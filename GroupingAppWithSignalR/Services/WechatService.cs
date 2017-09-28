using GroupingAppWithSignalR.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GroupingAppWithSignalR.Services
{
    public class WechatService
    {
        private static WechatService client = new WechatService();
        static string appkey = "wxe66445f68cbc9b8c";
        static string appsecret = "73693adedaa657fe6fcf8ea635547d33";
        static WechatService()
        {
        }
        WechatService()
        {
        }
        public static WechatService Instance
        {
            get
            {
                return client;
            }
        }
        public string AuthorizeWechatServer(string token,string timeStamp,string nonce)
        {
            return "";
        }
        public string BuildAuthorizeUrl(string state)
        {
            string callbackUri = "http://assigngroups.chinacloudsites.cn/Wechat/Callback";
            long timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            string sign = Md5Hash(appsecret + appkey);
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://open.weixin.qq.com/connect/oauth2/authorize?")
                      .Append("appid=").Append(appkey)
                      .Append("&redirect_uri=").Append(callbackUri)
                      .Append("&response_type=code")
                      .Append("&scope=snsapi_userinfo")
                      .Append("&state=").Append(state)
                      .Append("#wechat_redirect");
            return apiBuilder.ToString();
        }
        public string Authorization(string code, string state)
        {
            string result = "";
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://api.weixin.qq.com/sns/oauth2/access_token?appid=")
                      .Append(appkey).Append("&secret=").Append(appsecret)
                      .Append("&code=").Append(code).Append("&grant_type=authorization_code");
            try
            {
                HttpWebRequest httpget = (HttpWebRequest)WebRequest.Create(apiBuilder.ToString());
                //  httpget.Timeout = 2000;
                httpget.Method = "GET";
                HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse();
                using(StreamReader myStreamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    result = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                }
                
            }catch(Exception)
            {
                result = "";
            }
            return result;
        }
        public string GetUserInfo(string accessToken, string openid)
        {
            //https://api.weixin.qq.com/sns/userinfo?access_token=ACCESS_TOKEN&openid=OPENID&lang=zh_CN
            string result = "";
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://api.weixin.qq.com/sns/userinfo?access_token=")
                      .Append(accessToken).Append("&openid=").Append(openid).Append("&lang=zh_CN");
            try
            {
                HttpWebRequest httpget = (HttpWebRequest)WebRequest.Create(apiBuilder.ToString());
                //  httpget.Timeout = 2000;
                httpget.Method = "GET";
                using(HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse())
                {
                    using (StreamReader myStreamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        result = myStreamReader.ReadToEnd();

                        myStreamReader.Close();
                    }
                }
               
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }
        public void UpdateWechatInfo(WechatInfo info)
        {
            if (StorageService.UserAuthData.Exists(i => i.OpenId == info.OpenId))
            {
                var wechatInfo = StorageService.UserAuthData.SingleOrDefault(i => i.OpenId == info.OpenId);
                wechatInfo = info;
            }
            else
            {
                StorageService.UserAuthData.Add(info);
            }
        }
        public Task<GetOneResult<WechatInfo>> GetWechatInfo(string unionId)
        {
            GetOneResult<WechatInfo> result = new GetOneResult<WechatInfo>() {
                Message = "找不到该用户"
            };
            var entity = StorageService.UserAuthData.SingleOrDefault(e => e.OpenId == unionId);
            if (entity != null)
            {
                result.Entity = entity;
                result.Status = 200;
                result.Success = true;
            }
            return Task.FromResult(result);
        }
        public Task<GetOneResult<RoomUser>> GetUser(string unionId)
        {
            GetOneResult<RoomUser> result = new GetOneResult<RoomUser>() { Message = "找不到该用户" };
            var entity = StorageService.RegisteredUsers.SingleOrDefault(u => u.OpenId == unionId);
            if (entity != null)
            {
                result.Entity = entity;
                result.Status = 200;
                result.Success = true;
            }
            return Task.FromResult(result);
        }
        public string WechatShareTicket(string accessToken)
        {
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=")
                      .Append(accessToken).Append("&type=jsapi");

            HttpWebRequest httpget = (HttpWebRequest)WebRequest.Create(apiBuilder.ToString());
            //  httpget.Timeout = 2000;
            httpget.Method = "GET";

            HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse();
            StreamReader myStreamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));

            string result = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            return result;
        }
        public string WechatShareApiLogin()
        {
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=")
                      .Append(appkey).Append("&secret=").Append(appsecret);

            HttpWebRequest httpget = (HttpWebRequest)WebRequest.Create(apiBuilder.ToString());
            //  httpget.Timeout = 2000;
            httpget.Method = "GET";
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse();
                StreamReader myStreamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string result = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                if (result.Contains("access_token"))
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                    return jo["access_token"].ToString();
                }
                return "";
            }
            catch (System.Net.WebException)
            {
                return "";
            }
        }
        public string GetWechatShareTicket(string accessToken)
        {
            StringBuilder apiBuilder = new StringBuilder();
            apiBuilder.Append("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token=")
                      .Append(accessToken).Append("&type=jsapi");

            HttpWebRequest httpget = (HttpWebRequest)WebRequest.Create(apiBuilder.ToString());
            //  httpget.Timeout = 2000;
            httpget.Method = "GET";

            HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse();
            StreamReader myStreamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));

            string result = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            return result;
        }
        private static string Md5Hash(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}