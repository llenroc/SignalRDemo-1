using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Models
{
    public class WechatTicket
    {
        public string Ticket { get; set; }
        public DateTime ExpireTime { get; set; } 
    }
}