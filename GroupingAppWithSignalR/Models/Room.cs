using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public string RoomLeader { get; set; }
        public List<RoomUser> Users { get; set; } = new List<RoomUser>();
    }
}