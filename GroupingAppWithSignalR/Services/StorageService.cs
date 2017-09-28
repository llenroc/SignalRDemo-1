using GroupingAppWithSignalR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Services
{
    public class StorageService
    {
        private static StorageService client = new StorageService();
        static StorageService()
        {
        }
        StorageService()
        {
        }
        public static StorageService Instance
        {
            get
            {
                return client;
            }
        }
        public Result UpdateUserLevel(RoomUser user)
        {
            var result = new Result();
            var storedUser = RegisteredUsers.SingleOrDefault(u => u.OpenId == user.OpenId);
            if (storedUser != null)
            {
                storedUser.UserLevel = user.UserLevel;
                storedUser.Stars = user.Stars;
                result.Status = 200;
                result.Success = true;
            }
            return result;
        }
        public static List<Room> RoomList = RoomList ?? new List<Room>();
        public static List<RoomUser> RegisteredUsers = RegisteredUsers ?? new List<RoomUser>();
        public static List<WechatInfo> UserAuthData = UserAuthData ?? new List<WechatInfo>();
        public static List<WechatTicket> AcquiredTickets = AcquiredTickets ?? new List<WechatTicket>();
    }
}