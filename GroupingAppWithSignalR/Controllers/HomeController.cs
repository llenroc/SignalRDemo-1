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
            var result = await roomService.CreateRoom(user);
            return Json(result);
        }
        public async Task<ActionResult> RenderRoomUsers(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return Content("id不对昂");
            return PartialView("~/Views/PartialViews/_partialRoomUsers.cshtml",await roomService.ListUsersInRoom(roomId));
        }
        public ActionResult GetRoom(string roomId)
        {
            return View();
        }
        public async Task<JsonResult> JoinRoom(string roomId, RoomUser user)
        {
            return Json(await roomService.JoinRoom(roomId, user));
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
    }
}