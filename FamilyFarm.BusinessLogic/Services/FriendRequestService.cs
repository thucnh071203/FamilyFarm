using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
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

        public FriendRequestService(IFriendRequestRepository requestRepository, IAccountRepository accountRepository)
        {
            _requestRepository = requestRepository;
            _accountRepository = accountRepository;
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

        public async Task<bool> AcceptFriendRequestAsync(string friendId)
        {
            return await _requestRepository.AcceptFriendRequestAsync(friendId);
        }

        public async Task<bool> RejectFriendRequestAsync(string friendId)
        {
            return await _requestRepository.RejectFriendRequestAsync(friendId);
        }
        public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
        {
            return await _requestRepository.SendFriendRequestAsync(senderId, receiverId);
        }
    }
}
