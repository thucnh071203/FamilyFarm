using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FamilyFarm.BusinessLogic.Services
{
    public class GroupMemberService : IGroupMemberService
    {
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IHubContext<FriendHub> _hub;
        private readonly IHubContext<NotificationHub> _notiHub;

        public GroupMemberService(IGroupMemberRepository groupMemberRepository, IHubContext<FriendHub> hub, IHubContext<NotificationHub> notiHub = null)
        {
            _groupMemberRepository = groupMemberRepository;
            _hub = hub;
            _notiHub = notiHub;
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

            return result;
        }

        public async Task<long> DeleteGroupMember(string groupMemberId)
        {
            var result = await _groupMemberRepository.DeleteGroupMember(groupMemberId);

            if (result > 0)
            {
                await _hub.Clients.All.SendAsync("GroupMemberUpdate");
            }

            return result;
        }


        public async Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId)

        {
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
    }
}
