using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository friendRepository;
        private readonly IAccountRepository accountRepository;
        public FriendService(IFriendRepository friendRepository, IAccountRepository accountRepository)
        {
            this.friendRepository = friendRepository;
            this.accountRepository = accountRepository;
        }

        public async Task<FriendResponseDTO?> GetListFriends(string accId)
        {
            if (string.IsNullOrEmpty(accId)) return null;
            //get account from username
            var acc = await accountRepository.GetAccountById(accId);

            var listFriend = await friendRepository.GetListFriends(acc.AccId, acc.RoleId);
            if (listFriend.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have friend!",
                    Count = 0,
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFriend)
                {
                    var listMutualFriend = await MutualFriend(accId, friend.AccId);
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
                        FriendStatus = "Friend",
                        MutualFriend = listMutualFriend.Count

                    };
                    listAcc.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Count = listAcc.Count,
                    Data = listAcc,
                };
            }

        }
        public async Task<FriendResponseDTO?> GetListFollower(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            //get account from username
            var acc = await accountRepository.GetAccountByUsername(username);


            var listFollower = await friendRepository.GetListFollower(acc.AccId);
            if (listFollower.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have follower!",
                    Count = 0,
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFollower)
                {
                    var listMutualFriend = await MutualFriend(acc.AccId, friend.AccId);
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
                        FriendStatus = "Following",
                        MutualFriend = listMutualFriend.Count

                    };
                    listAcc.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Count = listAcc.Count,
                    Data = listAcc,
                };
            }
        }
        public async Task<FriendResponseDTO?> GetListFollowing(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            //get account from username
            var acc = await accountRepository.GetAccountByUsername(username);
            string roleExpert = "68007b2a87b41211f0af1d57";

            var listFollowing = await friendRepository.GetListFollowing(acc.AccId, roleExpert);
            if (listFollowing.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have following!",
                    Count = 0,
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFollowing)
                {
                    var listMutualFriend = await MutualFriend(acc.AccId, friend.AccId);
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
                        FriendStatus = "Following",
                        MutualFriend = listMutualFriend.Count

                    };
                    listAcc.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Count = listAcc.Count,
                    Data = listAcc,
                };
            }
        }
        public async Task<bool> Unfriend(string senderId, string receiverId)
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId)) return false;
            //get account from ID
            var acc = await accountRepository.GetAccountById(senderId);
            var acc1 = await accountRepository.GetAccountById(receiverId);

            return await friendRepository.Unfriend(acc.AccId, acc1.AccId);
        }
        public async Task<FriendResponseDTO?> MutualFriend(string userId, string otherId)
        {
            var acc = await accountRepository.GetAccountById(userId);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherId)) return null;
            var friendOfUser = await friendRepository.GetListFriends(userId, acc.RoleId);
            var friendOfOther = await friendRepository.GetListFriends(otherId, acc.RoleId);
            var commonAccounts = friendOfUser.Where(a1 => friendOfOther.Any(a2 => a2.AccId == a1.AccId)).ToList();
            if (commonAccounts.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have mutual friend!",
                    Count = 0,
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in commonAccounts)
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

                    };
                    listAcc.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Count = listAcc.Count,
                    Data = listAcc,
                };
            }
        }

        public async Task<FriendResponseDTO?> GetListSuggestionFriends(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            var list = await friendRepository.GetListSuggestionFriends(userId);
            if (list.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have friend suggestion!",
                    Count = 0,
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in list)
                {
                    var listMutualFriend = await MutualFriend(userId, friend.AccId);
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
                        MutualFriend = listMutualFriend.Count,
                    };
                    listAcc.Add(friendMapper);
                }
                return new FriendResponseDTO
                {
                    IsSuccess = true,
                    Count = listAcc.Count,
                    Data = listAcc,
                };
            }

        }
    }
}
