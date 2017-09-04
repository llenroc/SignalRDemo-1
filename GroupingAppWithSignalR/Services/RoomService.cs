using GroupingAppWithSignalR.Hubs;
using GroupingAppWithSignalR.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GroupingAppWithSignalR.Services
{
    public class RoomService
    {
        const int MaxRooms = 20;
        const int MaxUsersPerRoom = 10;
        private IHubConnectionContext<dynamic> HubClients { get; set; }

        private readonly static Lazy<RoomService> client = new Lazy<RoomService>(() => new RoomService(GlobalHost.ConnectionManager.GetHubContext<GroupMappingHub>().Clients));
        private RoomService(IHubConnectionContext<dynamic> clients)
        {
            HubClients = clients;
        }
        public static RoomService Instance
        {
            get
            {
                return client.Value;
            }
        }
        static string GenerateRoomId()
        {
            var rand = new Random();
            string id = "";
            while (true)
            {
                id = rand.Next(100000, 999999).ToString();
                if (StorageService.RoomList.Find(r => r.RoomId == id) == null)
                    break;
            }
            return id;
        }
        public Task<Room> CreateRoom(RoomUser user)
        {
            var room = new Room();
            room.RoomId = GenerateRoomId();
            room.RoomLeader = user.UserName;
            room.Users.Add(user);
            StorageService.RoomList.Add(room);
            return Task.FromResult(room);
        }
        public Task<Result> JoinRoom(string roomId,RoomUser user)
        {
            var result = new Result();
            var room = StorageService.RoomList.SingleOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                if (room.Users.Count() > MaxUsersPerRoom)
                {
                    result.Message = "房间人数已满";
                }
                else
                {
                    if (room.Users.Exists(r => r.UserName == user.UserName))
                    {
                        var host = room.Users.SingleOrDefault(u => u.UserName == user.UserName);
                        host.ConnectionId = user.ConnectionId;
                        host.RoomId = roomId;
                        result.Status = 303;
                        result.Message = "请不要重复加入房间";
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(user.UserName))
                        {
                            user.RoomId = roomId;
                            room.Users.Add(user);
                            result.Status = 200;
                            result.Success = true;
                        }
                       
                    }
                }
            }
            else
            {
                result.Message = "房间已销毁";
            }
            return Task.FromResult(result);
        }
        public Task<Result> LeaveRoom(RoomUser user)
        {
            var result = new Result();
            var prevRoom = StorageService.RoomList.SingleOrDefault(r => r.RoomId == user.RoomId);
            if (prevRoom != null)
            {
                prevRoom.Users.Remove(user);
                result.Status = 200;
                result.Success = true;
            }
            return Task.FromResult(result);
                
        }
        public Task RecycleRoom()
        {
            while (true)
            {
                Thread.Sleep(10000);
                for (int i = 0; i < StorageService.RoomList.Count(); i++)
                {
                    if (StorageService.RoomList[i].Users.Count() == 0)
                    {
                        StorageService.RoomList.Remove(StorageService.RoomList[i]);
                    }
                }
            }
        }
        public Task<Result> KickUser(string roomId,string roomLeaderName,string kickUserName)
        {
            var result = new Result();
            var room = StorageService.RoomList.SingleOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                if(room.RoomLeader == roomLeaderName)
                {
                    if (roomLeaderName == kickUserName)
                    {
                        result.Message = "踹自己，你彪？";
                    }
                    else
                    {
                        room.Users.RemoveAll(u => u.UserName == kickUserName);
                        result.Status = 200;
                        result.Success = true;
                    }
                }
                else
                {
                    result.Message = "你不是房主啊亲";
                }
            }
            else
            {
                result.Message = "房间已销毁";
            }
            return Task.FromResult(result);
        }
        public Task<Result> PromoteUser(string roomId,string roomLeaderName,string promoteUserName)
        {
            var result = new Result();
            var room = StorageService.RoomList.SingleOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                if (room.RoomLeader == roomLeaderName)
                {
                    if (roomLeaderName == promoteUserName)
                    {
                        result.Message = "你已经是房主了，莫装逼";
                    }
                    else
                    {
                        if (room.Users.Exists(u => u.UserName == promoteUserName))
                        {
                            room.RoomLeader = promoteUserName;
                            result.Status = 200;
                            result.Success = true;
                        }
                        else
                        {
                            result.Message = "找不到此人";
                        }
                        
                    }
                }
                else
                {
                    result.Message = "你不是房主啊亲";
                }
            }
            else
            {
                result.Message = "房间已销毁";
            }
            return Task.FromResult(result);
        }
        public Task<List<Room>> ListRooms()
        {
            return Task.FromResult(StorageService.RoomList);
        }
        public Task<List<RoomUser>> ListUsersInRoom(string roomId)
        {
            var room = StorageService.RoomList.SingleOrDefault(l => l.RoomId == roomId);
            var users = new List<RoomUser>();
            if (room != null)
                users = room.Users;
            return Task.FromResult(users);
        }
        public Task<List<RoomUser>> RandomGrouping(string roomId)
        {
            var room = StorageService.RoomList.SingleOrDefault(l => l.RoomId == roomId);
            var groupedList = new List<RoomUser>();
            var rand = new Random();
            if (room != null)
            {
                if (room.Users.Count() % 2 == 0)
                {
                    foreach(var user in room.Users)
                    {
                        int index = rand.Next(groupedList.Count());
                        groupedList.Insert(index, user);
                    }
                }
            }
            return Task.FromResult(groupedList);
        }
        public Task<List<RoomUser>> MMRGrouping(string roomId)
        {
            var room = StorageService.RoomList.SingleOrDefault(l => l.RoomId == roomId);
            var groupedList = new List<RoomUser>();
            if (room != null)
            {
                if (room.Users.Count() % 2 == 0)
                {
                    groupedList = room.Users.ToList();
                    for(var i = 0; i < groupedList.Count(); i++)
                    {
                        groupedList[i].MMR = (int)groupedList[i].UserLevel * 5 + groupedList[i].Stars;
                    }
                    groupedList = groupedList.OrderByDescending(u => u.MMR).ToList();
                }
            }
            return Task.FromResult(groupedList); 
        }
        
        public Task ResetRooms()
        {
            StorageService.RoomList = new List<Room>();
            return Task.FromResult(0);
        }
    }
}