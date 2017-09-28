using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Models
{
    public class WechatInfo
    {
        public string OpenId { get; set; }
        public string Access_Token { get; set; }
        public string Expires_In { get; set; }
        public string Refresh_Token { get; set; }
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}