using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FamilyFarm.BusinessLogic.Services
{
    public class GroupMemberService : IGroupMemberService
    {
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IHubContext<FriendHub> _hub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly INotificationService _notificationService;
        private readonly IAccountRepository _accountRepository;

        public GroupMemberService(IGroupMemberRepository groupMemberRepository, IHubContext<FriendHub> hub, IHubContext<NotificationHub> notificationHub = null, INotificationService notificationService = null, IGroupRepository groupRepository = null, IAccountRepository accountRepository = null)
        {
            _groupMemberRepository = groupMemberRepository;
            _hub = hub;
            _notificationHub = notificationHub;
            _notificationService = notificationService;
            _groupRepository = groupRepository;
            _accountRepository = accountRepository;
        }

        public async Task<GroupMember> GetGroupMemberById(string groupMemberId)
        {
            return await _groupMemberRepository.GetGroupMemberById(groupMemberId);
        }

        public async Task<GroupMember> AddGroupMember(string groupId, string accountId, string inviterId)
        {
            var result = await _groupMemberRepository.AddGroupMember(groupId, accountId, inviterId);

            if (result != null)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }

            var getMemberJoined = await _groupMemberRepository.GetMemberJoinedGroup(groupId, accountId);

            var getGroup = await _groupRepository.GetGroupById(groupId);

            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { accountId },
                SenderId = inviterId,
                CategoryNotiId = "6885d3c4dc84c1d03fefd952",
                TargetId = getMemberJoined.GroupMemberId,
                TargetType = "GroupMember", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "added you to the group " + getGroup?.GroupName + "."
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }

            return result;
        }

        public async Task<long> DeleteGroupMember(string groupMemberId, string deleterId)
        {
            var groupMember = await _groupMemberRepository.GetGroupMemberById(groupMemberId);
            var group = await _groupRepository.GetGroupById(groupMember.GroupId);

            var result = await _groupMemberRepository.DeleteGroupMember(groupMemberId);

            if (result > 0)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }
            //notification
            var acc = await _accountRepository.GetAccountByIdAsync(deleterId);
            //var list = account.Select(a => a.AccId).ToList();
            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { groupMember.AccId },
                SenderId = deleterId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c3f",
                TargetId = group.GroupId,
                TargetType = "Group", //để link tới notifi gốc Post, Chat, Process, ...
                Content = acc.FullName + " deleted you out of " + group.GroupName + ".",
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }
            return result;
        }

        public async Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId, string accId)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentNullException(nameof(groupId), "GroupId is required");

            var checkMemberJoinedGroup = await _groupMemberRepository.GetMemberJoinedGroup(groupId, accId);

            if (checkMemberJoinedGroup == null)
            {
                throw new UnauthorizedAccessException("You are not a member of this group.");
            }

            return await _groupMemberRepository.GetUsersInGroupAsync(groupId);
        }

        public async Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword)
        {
            return await _groupMemberRepository.SearchUsersInGroupAsync(groupId, keyword);
        }
   
        public async Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId)
        {
            return await _groupMemberRepository.GetJoinRequestsAsync(groupId);
        }

        public async Task<GroupMember?> RequestToJoinGroupAsync(string accId, string groupId)
        {
            var checkMember = await _groupMemberRepository.GetMemberInvitedOrJoinedGroup(accId, groupId);
            if (checkMember != null)
            {
                return null;
            }

            var result = await _groupMemberRepository.RequestToJoinGroupAsync(accId, groupId);

            if (result != null)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }

            return result;
        }

        public async Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus)
        {
           
            var result = await _groupMemberRepository.RespondToJoinRequestAsync(groupMemberId, responseStatus);

            if (result)
            {
                // Chỉ gửi tín hiệu khi xử lý DB thành công
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }

            return result;
        }

        public async Task<bool> UpdateMemberRoleAsync(string groupMemberId, string newGroupRoleId)
        {
            var result = await _groupMemberRepository.UpdateMemberRoleAsync(groupMemberId, newGroupRoleId);
            if (result)
            {
                var groupMember = await _groupMemberRepository.GetGroupMemberById(groupMemberId);
                var accId = groupMember.AccId;
                var groupId = groupMember.GroupId;

                // Gửi cho toàn nhóm để reload danh sách thành viên
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");

                // Gửi riêng cho người bị đổi quyền
                await _hub.Clients.All.SendAsync("RoleChanged", groupId, accId, newGroupRoleId);
                Console.WriteLine($"🔔 Sending RoleChanged → accId: {accId}, groupId: {groupId}, newRole: {newGroupRoleId}");

            }
            return result;
        }


        public async Task<bool> LeaveGroupAsync(string groupId, string accId)
        {
            var result = await _groupMemberRepository.LeaveGroupAsync(groupId, accId);
            if (result)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }
            return result;
        }

        public async Task<GroupMemberResponseDTO> GetOneUserInGroupAsync(string groupId, string accId)
        {
            var listMember = await _groupMemberRepository.GetUsersInGroupAsync(groupId);

            var member = listMember.FirstOrDefault(m => m.AccId == accId);

            return member;
        }

        public async Task<GroupMember> InviteGroupMember(string groupId, string accountId, string inviterId)
        {
            var result = await _groupMemberRepository.InviteMember(groupId, accountId, inviterId);

            if (result != null)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }

            var getMemberJoined = await _groupMemberRepository.GetMemberInvitedGroup(groupId, accountId);

            var getGroup = await _groupRepository.GetGroupById(groupId);

            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { accountId },
                SenderId = inviterId,
                CategoryNotiId = "6885d3c4dc84c1d03fefd952",
                TargetId = getMemberJoined.GroupMemberId,
                TargetType = "GroupMember", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "invited you to the group " + getGroup?.GroupName + "."
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }

            return result;
        }

        //public async Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus)
        //{
        //    var result = false;
        //    if (responseStatus.Equals("Accept"))
        //    {
        //        var getGroupMember = await _groupMemberRepository.GetGroupMemberInviteById(groupMemberId);

        //        var getGroup = await _groupRepository.GetGroupById(getGroupMember.GroupId);

        //        if (getGroup.PrivacyType.Equals("Public"))
        //        {
        //            result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, "Accept");
        //        } else if (getGroup.PrivacyType.Equals("Private"))
        //        {
        //            result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, "Pending");
        //        }
        //    } else
        //    {
        //        result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, responseStatus);
        //    }

        //    if (result)
        //    {
        //        // Chỉ gửi tín hiệu khi xử lý DB thành công
        //        await _hub.Clients.All.SendAsync("GroupMemberUpdate");
        //    }

        //    return result;
        //}

        public async Task<(bool Success, string? Message)> RespondToInviteRequestAsync(string groupMemberId, string responseStatus)
        {
            var result = false;

            if (responseStatus.Equals("Accept", StringComparison.OrdinalIgnoreCase))
            {
                var getGroupMember = await _groupMemberRepository.GetGroupMemberInviteById(groupMemberId);
                if (getGroupMember == null)
                    return (false, "The invite request no longer exists");

                var getGroup = await _groupRepository.GetGroupById(getGroupMember.GroupId);
                if (getGroup == null)
                {
                    await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, "Reject");
                    await _hub.Clients.All.SendAsync("GroupMemberUpdate");
                    return (false, "The group you respond has unavailable");
                }

                if (string.Equals(getGroup.PrivacyType, "Public", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, "Accept");
                }
                else if (string.Equals(getGroup.PrivacyType, "Private", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, "Pending");
                }
                else
                {
                    return (false, "Invalid group privacy type");
                }
            }
            else
            {
                // Reject hoặc các trạng thái khác
                result = await _groupMemberRepository.RespondToInviteRequestAsync(groupMemberId, responseStatus);
            }

            if (result)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
                return (true, null); // hoặc trả message thành công tùy bạn
            }

            return (false, "Invalid request or status");
        }

        public async Task<GroupMember> GetMemberInvitedOrJoinedGroup(string groupId, string accId)
        {
            return await _groupMemberRepository.GetMemberInvitedOrJoinedGroup(groupId, accId);
        }

        public async Task<GroupMember> GetGroupMemberInviteById(string groupMemberId)
        {
            return await _groupMemberRepository.GetGroupMemberInviteById(groupMemberId);
        }
    }
}
