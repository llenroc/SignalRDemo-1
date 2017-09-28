using GroupingAppWithSignalR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GroupingAppWithSignalR.Models;
using System.Web.Caching;

namespace GroupingAppWithSignalR.Controllers
{
    public class WechatController : Controller
    {
        // GET: Wechat
        public ActionResult Index()
        {
            return View();
        }
        public string AuthorizeWechatServer()
        {
            return Request.QueryString["echostr"];
        }
        public void Login(string state)
        {
            string url = WechatService.Instance.BuildAuthorizeUrl(state);
            Response.Redirect(url);
        }
        public async Task<ActionResult> CallBack(string code , string state)
        {
            string message = WechatService.Instance.Authorization(code, state);
            if (message.Contains("access_token"))
            {
                try
                {
                    //获取并解析Access_Token json包
                    WechatInfo info = JsonConvert.DeserializeObject<WechatInfo>(message);
                    //更新用户token
                    WechatService.Instance.UpdateWechatInfo(info);

                    //通过Access_Token获取用户信息
                    string userInfo = WechatService.Instance.GetUserInfo(info.Access_Token, info.OpenId);
                    //判定userInfo是否合法
                    if (!string.IsNullOrWhiteSpace(userInfo) && userInfo.Contains("nickname"))
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(userInfo);
                        //判断该用户是否存在
                        var result = await WechatService.Instance.GetUser(info.OpenId);
                        string userData = "";
                        if (!result.Success)
                        {
                            //不存在,创建用户并存储
                            RoomUser user = new RoomUser()
                            {
                                OpenId = info.OpenId,
                                Avatar = jo["headimgurl"].ToString(),
                                UserName = jo["nickname"].ToString(),
                            };
                            StorageService.RegisteredUsers.Add(user);
                            //编辑要写入的cookie值
                            userData = JsonConvert.SerializeObject(user);
                        }
                        else
                        {
                            //编辑要写入的cookie值
                            userData = JsonConvert.SerializeObject(result.Entity);
                        }

                        //写入cookie
                        HttpCookie cookie = new HttpCookie("userData");
                        cookie.Value = HttpUtility.UrlEncode(userData);
                        cookie.Expires = DateTime.Now.AddDays(25);
                        if (Request.Cookies["userData"] != null)
                        {
                            Response.Cookies.Set(cookie);
                        }
                        else
                        {
                            Response.Cookies.Add(cookie);
                        }
                        
                        if (Convert.ToInt32(state) < 1000)
                        {
                            return RedirectToAction("RoomList", "Home");
                        }
                        else
                        {
                            return RedirectToAction("GetRoom", "Home", new { roomId = state, openId = info.OpenId });
                        }
                        
                    }
                    return RedirectToAction("RoomList", "Home", new { message = HttpUtility.UrlEncode(userInfo) });
                }
                catch(Exception e)
                {
                    return RedirectToAction("RoomList", "Home", new { message = HttpUtility.UrlEncode(e.Message) });
                }

            }
            else
            {
                return RedirectToAction("RoomList", "Home", new { message = HttpUtility.UrlEncode(message) });
            }
        }

        public string GetSignature()
        {
            var latestTicket = StorageService.AcquiredTickets.OrderByDescending(t => t.ExpireTime).FirstOrDefault();
            if (latestTicket != null && latestTicket.ExpireTime > DateTime.Now)
            {
                if (HttpRuntime.Cache["wechatTicket"] == null)
                {
                    HttpRuntime.Cache.Add("wechatTicket", latestTicket.Ticket, null, latestTicket.ExpireTime, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
                }
            }
            else
            {
                var accessToken = WechatService.Instance.WechatShareApiLogin();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var ticketMsg = WechatService.Instance.GetWechatShareTicket(accessToken);
                    if (ticketMsg.Contains("ticket"))
                    {
                        JObject jo = (JObject)JsonConvert.DeserializeObject(ticketMsg);
                        WechatTicket ticket = new WechatTicket()
                        {
                            Ticket = jo["ticket"].ToString(),
                            ExpireTime = DateTime.Now.AddSeconds(7000)
                        };
                        StorageService.AcquiredTickets.Add(ticket);
                        HttpRuntime.Cache.Add("wechatTicket", ticket.Ticket, null, ticket.ExpireTime, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
                    }
                }
                else
                {
                    return "";
                }
            }
            return HttpRuntime.Cache["wechatTicket"].ToString();
        }
    }
}