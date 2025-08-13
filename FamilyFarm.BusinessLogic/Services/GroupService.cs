using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyFarm.BusinessLogic.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _memberRepository;
        private readonly IUploadFileService _uploadFileService;
        private readonly IHubContext<NotificationHub> _hubNotificationContext;
        private readonly IHubContext<FriendHub> _hub;
        private readonly INotificationService _notificationService;

        public GroupService(IGroupRepository groupRepository, IGroupMemberRepository memberRepository, IUploadFileService uploadFileService, IHubContext<NotificationHub> hubNotificationContext, IHubContext<FriendHub> hub, INotificationService notificationService)
        {
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
            _uploadFileService = uploadFileService;
            _hubNotificationContext = hubNotificationContext;
            _hub = hub;
            _notificationService = notificationService;
        }

        public async Task<GroupResponseDTO> GetAllGroup()
        {
            var listAllGroup = await _groupRepository.GetAllGroup();

            if (listAllGroup.Count == 0 || listAllGroup == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group list is empty"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get all group successfully",
                Count = listAllGroup.Count,
                Data = listAllGroup
            };
        }

        public async Task<GroupCardResponseDTO> GetAllByUserId(string userId)
        {
            var groupCards = await _groupRepository.GetAllByUserId(userId);

            if (groupCards == null || groupCards.Count == 0)
            {
                return new GroupCardResponseDTO
                {
                    Success = false,
                    Message = "User has not joined any group",
                    Count = 0,
                    Data = new List<GroupCardDTO>()
                };
            }

            return new GroupCardResponseDTO
            {
                Success = true,
                Message = "Get joined groups successfully",
                Count = groupCards.Count,
                Data = groupCards
            };
        }

        public async Task<GroupResponseDTO> GetGroupById(string groupId)
        {
            var group = await _groupRepository.GetGroupById(groupId);

            if (group == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group not found"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Data = new List<Group> { group },
            };

        }

        public async Task<GroupResponseDTO> CreateGroup(GroupRequestDTO item)
        {
            if (item == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            //var bgURL = await _uploadFileService.UploadImage(item.GroupBackground);

            //var avtURL = await _uploadFileService.UploadImage(item.GroupAvatar);

            string backgroundUrl = "";
            if (item.GroupBackground != null)
            {
                var bgURL = await _uploadFileService.UploadImage(item.GroupBackground);
                backgroundUrl = bgURL?.UrlFile ?? "";
            }

            string avatarUrl = "";
            if (item.GroupAvatar != null)
            {
                var avtURL = await _uploadFileService.UploadImage(item.GroupAvatar);
                avatarUrl = avtURL?.UrlFile ?? "";
            }

            var addNewGroup = new Group
            {
                GroupId = null,
                GroupName = item.GroupName,
                GroupAvatar = avatarUrl,
                GroupBackground = backgroundUrl,
                PrivacyType = item.PrivacyType,
                OwnerId = item.AccountId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            };

            var created = await _groupRepository.CreateGroup(addNewGroup);

            if (created == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to create group"
                };
            }

            var getGroupId = await _groupRepository.GetLatestGroupByCreator(item.AccountId);

            if (getGroupId == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Get group id of owner failed."
                };
            }

            var addNewOwner = await _memberRepository.AddGroupOwner(getGroupId.GroupId, item.AccountId);

            if (addNewOwner == null) 
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Add owner failed."
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { created }
            };
        }

        public async Task<GroupResponseDTO> UpdateGroup(string groupId, GroupRequestDTO item)
        {
            if (item == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var checkOwner = await _groupRepository.GetGroupById(groupId);

            if (checkOwner.OwnerId != item.AccountId)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Provider does not match"
                };
            }

            string finalBgUrl = checkOwner.GroupBackground;

            if (item.GroupBackground != null)
            {
                var imageURL = await _uploadFileService.UploadImage(item.GroupBackground);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalBgUrl = imageURL.UrlFile;
                }
            }

            string finalAvtUrl = checkOwner.GroupAvatar;

            if (item.GroupAvatar != null)
            {
                var imageURL = await _uploadFileService.UploadImage(item.GroupAvatar);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalAvtUrl = imageURL.UrlFile;
                }
            }

            var updateGroup = new Group
            {
                GroupId = null,
                GroupName = item.GroupName,
                GroupAvatar = finalAvtUrl,
                GroupBackground = finalBgUrl,
                PrivacyType = item.PrivacyType,
                OwnerId = item.AccountId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            };

            var updated = await _groupRepository.UpdateGroup(groupId, updateGroup);

            if (updated == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to update group"
                };
            }

            //// Lấy danh sách thành viên (List<GroupMemberResponseDTO>)
            //var members = await _memberRepository.GetUsersInGroupAsync(groupId);

            //// Lấy danh sách accId từ danh sách thành viên và gửi signlR
            //foreach (var accId in members.Select(m => m.AccId).Distinct())
            //{
            //    await _hubNotificationContext.Clients.Group(accId).SendAsync("GroupUpdated", updated);
            //}

            // Sau khi cập nhật group thành công...
            var members = await _memberRepository.GetUsersInGroupAsync(groupId);
            var memberAccIds = members.Select(m => m.AccId).Distinct().ToList();

            // Log danh sách accId và group cập nhật
            Console.WriteLine($"[SignalR] Gửi GroupUpdated tới các accId: {string.Join(",", memberAccIds)}");
            Console.WriteLine($"[SignalR] Data gửi lên: {System.Text.Json.JsonSerializer.Serialize(updated)}");

            foreach (var accId in memberAccIds)
            {
                try
                {
                    Console.WriteLine($"[SignalR] Gửi tới accId: {accId}");
                    await _hubNotificationContext.Clients.Group(accId).SendAsync("GroupUpdated", updated);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Gửi tới accId {accId} lỗi: {ex.Message}");
                }
            }
            //notification
            var account = await _memberRepository.GetUsersInGroupAsync(groupId);
            var list = account.Select(a => a.AccId).ToList();
            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = list,
                SenderId = item.AccountId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3f",
                TargetId = groupId,
                TargetType = "Group", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "Group "+ item.GroupName +" updated!"
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }


            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group updated successfully",
                Data = new List<Group> { updated }
            };
        }

        public async Task<GroupResponseDTO> GetLatestGroupByCreator(string creatorId)
        {
            var group = await _groupRepository.GetLatestGroupByCreator(creatorId);

            if (group == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group not found"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Data = new List<Group> { group }
            };
        }

        public async Task<GroupResponseDTO> DeleteGroup(string groupId)
        {
            var group = await _groupRepository.GetGroupById(groupId);
            if (group == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group not found"
                };
            }
                //return BadRequest("Group not found");

            var deletedCount = await _groupRepository.DeleteGroup(groupId);


            if (deletedCount == 0)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to delete group"
                };
            }
            //notification
            var account = await _memberRepository.GetUsersInGroupAsync(groupId);
            var list = account.Select(a => a.AccId).ToList();
            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = list,
                SenderId = group.OwnerId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3f",
                TargetId = groupId,
                TargetType = "Group", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "Group " + group.GroupName + " deleted!"
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }

            var deleteAllMember = await _memberRepository.DeleteAllGroupMember(groupId);

            await _hub.Clients.All.SendAsync("GroupDeleted", groupId);

            if (deleteAllMember == -1)
            {
                return new GroupResponseDTO
                {
                    Success = true,
                    Message = "Delete all member failed."
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group deleted successfully",
                Data = null
            };
        }

        public async Task<GroupCardResponseDTO> GetGroupSuggestion(string userId, int number)
        {
            var groupCardList = await _groupRepository.GetGroupsSuggestion(userId, number);

            if (groupCardList == null || groupCardList.Count == 0)
            {
                return new GroupCardResponseDTO
                {
                    Success = false,
                    Message = "Group list is empty",
                    Count = 0,
                    Data = new List<GroupCardDTO>()
                };
            }

            return new GroupCardResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Count = groupCardList.Count,
                Data = groupCardList
            };
        }
        public async Task<GroupCardResponseDTO> SearchGroups(string userId, string searchTerm)
        {
            // Kiểm tra input
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new GroupCardResponseDTO
                {
                    Success = false,
                    Message = "Search term cannot be empty",
                    Count = 0,
                    Data = new List<GroupCardDTO>()
                };
            }

            var groupCardList = await _groupRepository.SearchGroups(userId, searchTerm.Trim());

            if (groupCardList == null || groupCardList.Count == 0)
            {
                return new GroupCardResponseDTO
                {
                    Success = true,
                    Message = "No groups found matching your search",
                    Count = 0,
                    Data = new List<GroupCardDTO>()
                };
            }

            return new GroupCardResponseDTO
            {
                Success = true,
                Message = $"Found {groupCardList.Count} group(s)",
                Count = groupCardList.Count,
                Data = groupCardList
            };
        }
    }
}
