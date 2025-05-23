﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IGroupMemberRepository
    {
        Task<GroupMember> GetGroupMemberById(string groupMemberId);
        Task<GroupMember> AddGroupMember(GroupMember groupMember);
        Task<long> DeleteGroupMember(string groupMemberId);
        Task<long> DeleteAllGroupMember(string groupId);

        Task<List<Account>> GetUsersInGroupAsync(string groupId);
        Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword);

        Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId);
        Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId);

        Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus);

        Task<bool> UpdateMemberRoleAsync(string groupId, string accId, string newGroupRoleId);


    }
}
