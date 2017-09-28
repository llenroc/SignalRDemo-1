using GroupingAppWithSignalR.Models;
using GroupingAppWithSignalR.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GroupingAppWithSignalR.Controllers
{
    public class HomeController : Controller
    {
        RoomService roomService;
        public HomeController()
        {
            roomService = RoomService.Instance;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Chat()
        {
            return View();
        }
        public async Task<ActionResult> RoomList()
        {
            var roomList =await roomService.ListRooms();
            return View(roomList);
        }
        [HttpPost]
        public async Task<JsonResult> CreateRoom(RoomUser user)
        {
            var result = new GetOneResult<Room>();
            if (ModelState.IsValid)
            {
                result = await roomService.CreateRoom(user);
            }
            return Json(result);
        }
        public async Task<ActionResult> RenderRoomUsers(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return Content("id不对昂");
            return PartialView("~/Views/PartialViews/_partialRoomUsers.cshtml",await roomService.ListUsersInRoom(roomId));
        }
        public ActionResult GetRoom(string roomId,string openId)
        {
            if (StorageService.RegisteredUsers.Exists(u => u.OpenId == openId))
            {
                return View();
            }
            return RedirectToAction("Login", "Wechat", new { state = roomId });
        }
        public async Task<JsonResult> JoinRoom(string roomId, RoomUser user)
        {
            var result = new Result();
            if (ModelState.IsValid)
            {
                result = await roomService.JoinRoom(roomId, user);
            }
            else
            {
                result.Message = "/Home/RoomList";
            }
            return Json(result);
        }
        public async Task<JsonResult> ResetRooms()
        {
            await roomService.ResetRooms();
            return Json(0,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllRooms()
        {
            return Json(StorageService.RoomList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateUserLevel(RoomUser user)
        {
            return Json(StorageService.Instance.UpdateUserLevel(user));
        }
    }
}