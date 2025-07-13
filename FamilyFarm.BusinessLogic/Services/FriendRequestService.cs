using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRequestRepository _requestRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IHubContext<FriendHub> _hub;
        private readonly INotificationService _notificationService;

        public FriendRequestService(IFriendRequestRepository requestRepository, IAccountRepository accountRepository, IHubContext<FriendHub> hub, INotificationService notificationService = null)
        {
            _requestRepository = requestRepository;
            _accountRepository = accountRepository;
            _hub = hub;
            _notificationService = notificationService;
        }
        public async Task<FriendResponseDTO?> GetAllSendFriendRequests(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            var account = await _accountRepository.GetAccountByUsername(username);

            var listReceiveRequest = await _requestRepository.GetSentFriendRequests(account.AccId);
            if (listReceiveRequest.Count == 0)
            {
                return new FriendResponseDTO
                {
                    IsSuccess = false,
                    Message = "Khong co loi moi ket ban nao!",

                };
            }
            else
            {
                List<FriendMapper> listSent = new List<FriendMapper>();
                foreach (var friend in listReceiveRequest)
                {
                    var friendMapper = new FriendMapper
                    {
                        AccId = friend.AccId,
                        RoleId = friend.RoleId,
                        Username = friend.Username,
                        FullName = friend.FullName,
                        Birthday = friend.Birthday,
                        Gender = friend.Gender,
                        City = friend.City,
                        Country = friend.Country,
                        Address = friend.Address,
                        Avatar = friend.Avatar,
                        Background = friend.Background,
                        Certificate = friend.Certificate,
                        WorkAt = friend.WorkAt,
                        StudyAt = friend.StudyAt,
                        Status = friend.Status,
                        FriendStatus = "Pending"


                    };
                    listSent.Add(friendMapper);
                }
                return new FriendResponseDTO
                {

                    IsSuccess = true,
                    Message = "Loi moi ket bạn!",
                    Data = listSent,
                };
            }
        }
        public async Task<FriendResponseDTO?> GetAllReceiveFriendRequests(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            var account = await _accountRepository.GetAccountByUsername(username);
            var listSendRequest = await _requestRepository.GetReceiveFriendRequests(account.AccId);
            if (listSendRequest.Count == 0)
            {
                return new FriendResponseDTO
                {
                    IsSuccess = false,
                    Message = "Khong có loi moi nao gửi đi!",
                };
            }
            else
            {
                List<FriendMapper> listReceive = new List<FriendMapper>();
                foreach (var friend in listSendRequest)
                {
                    var friendMapper = new FriendMapper
                    {
                        AccId = friend.AccId,
                        RoleId = friend.RoleId,
                        Username = friend.Username,
                        FullName = friend.FullName,
                        Birthday = friend.Birthday,
                        Gender = friend.Gender,
                        City = friend.City,
                        Country = friend.Country,
                        Address = friend.Address,
                        Avatar = friend.Avatar,
                        Background = friend.Background,
                        Certificate = friend.Certificate,
                        WorkAt = friend.WorkAt,
                        StudyAt = friend.StudyAt,
                        Status = friend.Status,
                        FriendStatus = "Pending"

                    };
                    listReceive.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Message = "So loi moi da gửi!",
                    Data = listReceive,


                };
            }
        }

        public async Task<bool> AcceptFriendRequestAsync(string senderId, string receiverId)
        {
            
            var result= await _requestRepository.AcceptFriendRequestAsync(senderId, receiverId);
            if (result)
            {
                await _hub.Clients.All.SendAsync("FriendUpdate"); //  đặt sau khi xử lý DB thành công
            }
            //notification
            var account = await _accountRepository.GetAccountByAccId(receiverId);
            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { receiverId },
                SenderId = senderId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3d",
                TargetId = receiverId,
                TargetType = "Friend", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "You have a notification about friend relationship from " + account?.FullName
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }
            return result;
        }

        public async Task<bool> RejectFriendRequestAsync(string senderId, string receiverId)
        {
            // Gửi thông báo SignalR
            await _hub.Clients.All.SendAsync("FriendUpdate");
            var result = await _requestRepository.RejectFriendRequestAsync(senderId, receiverId);
            if (result)
            {
                await _hub.Clients.All.SendAsync("FriendUpdate"); //  đặt sau khi xử lý DB thành công
            }
            return result;
        }
        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
        {
            // Gửi thông báo SignalR
            await _hub.Clients.All.SendAsync("FriendUpdate");
            var result = await _requestRepository.SendFriendRequestAsync(senderId, receiverId);
            if (result)
            {
                await _hub.Clients.All.SendAsync("FriendUpdate"); // đặt sau khi xử lý DB thành công
            }
            var account = await _accountRepository.GetAccountByAccId(receiverId);
            //send notifi
            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { receiverId },
                SenderId = senderId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3d",
                TargetId = receiverId,
                TargetType = "Friend", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "You have a notification about friend relationship from " + account?.FullName
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }
            return result;
        }
    }
}
