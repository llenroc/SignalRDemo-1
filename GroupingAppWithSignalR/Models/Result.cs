using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupingAppWithSignalR.Models
{
    public class Result
    {
        public bool Success { get; set; } = false;
        public int Status { get; set; } = 400;
        public string Message { get; set; } = "";
    }
    public class GetOneResult<T> :Result where T : class, new()
    {
        public T Entity { get; set; }
    }
}