using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using GroupingAppWithSignalR.Services;
using GroupingAppWithSignalR.Models;

namespace GroupingAppWithSignalR.Hubs
{
    public class GroupMappingHub : Hub
    {
        RoomService roomService = RoomService.Instance;
        public void SendToAll(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage("[综合]" + name, message);
        }
        public void SendToGroup(string name, string message, string roomName)
        {
            Clients.Group(roomName).addNewMessageToPage("[房间]" + name, message);
        }
        public async Task JoinRoom(string roomId, RoomUser user)
        {
           
            user.ConnectionId = Context.ConnectionId;
            var result = await roomService.JoinRoom(roomId, user);
            if (result.Status == 200 || result.Status == 303)
            {
                await Groups.Add(Context.ConnectionId, roomId);
                var room = StorageService.RoomList.SingleOrDefault(r => r.RoomId == roomId);
                if (room.LeaderId == user.OpenId)
                    Clients.Client(user.ConnectionId).renderRLActions(true);
                Clients.Group(roomId).addNewMessageToPage("[房间]" + user.UserName, " 进入了房间");
                Clients.Group(roomId).renderUserList();
            }
            else
            {
                Clients.Client(user.ConnectionId).showErrorMessage(result.Message);
            }
           
        }

        public async Task LeaveRoom(string connectionId)
        {
            var roomList = StorageService.RoomList;
            RoomUser user = null;
            foreach(var room in roomList)
            {
                if (room.Users.Exists(u => u.ConnectionId == connectionId))
                {
                    user = room.Users.SingleOrDefault(u => u.ConnectionId == connectionId);
                    break;
                }
            }
            if (user != null)
            {
                var result = await roomService.LeaveRoom(user);
                if (result.Success)
                {
                    Clients.Group(user.RoomId).addNewMessageToPage("[房间]" + user.UserName, " 离开了房间");
                    Clients.Group(user.RoomId).renderUserList();
                    await Groups.Remove(Context.ConnectionId, user.RoomId);
                }
            }
        }

        public async Task RandomGrouping(string roomId)
        {
            var groupedList = await roomService.RandomGrouping(roomId);
            Clients.Group(roomId).renderGroup(groupedList, 1);
        }
        public async Task MMRGrouping(string roomId)
        {
            var groupedList = await roomService.MMRGrouping(roomId);
            Clients.Group(roomId).renderGroup(groupedList, 2);
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Task.Factory.StartNew(() => LeaveRoom(Context.ConnectionId));
            return base.OnDisconnected(stopCalled);
        }
    }
}