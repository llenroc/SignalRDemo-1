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
        public static List<Room> RoomList = RoomList ?? new List<Room>();
    }
}