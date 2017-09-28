using GroupingAppWithSignalR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Models
{
    public class RoomUser
    {
        [Required]
        public string UserName { get; set; }
        public int Stars { get; set; } 
        public UserLevel UserLevel { get; set; }
        public string RoomId { get; set; }
        public string ConnectionId { get; set; }
        public int MMR { get; set; } = 0;
        [Required]
        public string Avatar { get; set; }
        [Required]
        public string OpenId { get; set; }

    }
}