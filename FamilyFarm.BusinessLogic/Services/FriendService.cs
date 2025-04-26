using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using System;
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

        public async Task<FriendResponseDTO?> GetListFriends(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            //get account from username
            var acc = await accountRepository.GetAccountByUsername(username);

            var listFriend = await friendRepository.GetListFriends(acc.AccId);
            if (listFriend.Count == 0) {
                return new FriendResponseDTO
                {
                    Message = "You dont have friend!",
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFriend)
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
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFollower)
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
                    Data = listAcc,
                };
            }
        }
        public async Task<FriendResponseDTO?> GetListFollowing(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            //get account from username
            var acc = await accountRepository.GetAccountByUsername(username);


            var listFollowing = await friendRepository.GetListFollowing(acc.AccId);
            if (listFollowing.Count == 0)
            {
                return new FriendResponseDTO
                {
                    Message = "You dont have following!",
                    IsSuccess = false,
                };
            }
            else
            {
                List<FriendMapper> listAcc = new List<FriendMapper>();
                foreach (var friend in listFollowing)
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
                    Data = listAcc,
                };
            }
        }
        public async Task<bool> Unfriend(string sender, string receiver)
        {
            if (string.IsNullOrEmpty(sender)||string.IsNullOrEmpty(receiver)) return false;
            //get account from username
            var acc = await accountRepository.GetAccountByUsername(sender);
            var acc1 = await accountRepository.GetAccountByUsername(receiver);

            return await friendRepository.Unfriend(acc.AccId, acc1.AccId);
        }

    }
}
