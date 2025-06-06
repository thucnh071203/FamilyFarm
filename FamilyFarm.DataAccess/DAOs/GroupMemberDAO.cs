﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class GroupMemberDAO
    {
        private readonly IMongoCollection<GroupMember> _GroupMembers;
        private readonly IMongoCollection<Account> _Accounts;
        public GroupMemberDAO(IMongoDatabase database)
        {
            _GroupMembers = database.GetCollection<GroupMember>("GroupMember");
            _Accounts = database.GetCollection<Account>("Account");
        }


        public async Task<GroupMember> GetByIdAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return null;

            return await _GroupMembers.Find(g => g.GroupMemberId == groupMemberId && g.MemberStatus.Equals("Accept")).FirstOrDefaultAsync();
        }

        public async Task<GroupMember> AddAsync(GroupMember groupMember)
        {
            groupMember.GroupMemberId = ObjectId.GenerateNewId().ToString();
            groupMember.GroupRoleId = groupMember.GroupRoleId;
            groupMember.GroupId = groupMember.GroupId;
            groupMember.AccId = groupMember.AccId;
            groupMember.JointAt = DateTime.UtcNow;
            groupMember.MemberStatus = "Accept";
            if (!string.IsNullOrEmpty(groupMember.InviteByAccId) && ObjectId.TryParse(groupMember.InviteByAccId, out _))
            {
                groupMember.InviteByAccId = groupMember.InviteByAccId;
            }
            else
            {
                groupMember.InviteByAccId = null;
            }
            groupMember.LeftAt = null;

            await _GroupMembers.InsertOneAsync(groupMember);
            return groupMember;
        }

        public async Task<long> DeleteAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return 0;

            var filter = Builders<GroupMember>.Filter.Eq(g => g.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept");

            if (filter == null) return 0;

            var update = Builders<GroupMember>.Update
                .Set(g => g.MemberStatus, "Left")
                .Set(g => g.LeftAt, DateTime.UtcNow);

            var result = await _GroupMembers.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }

        public async Task<long> DeleteAllAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return 0;

            var filter = Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId) &
                         Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept");

            var update = Builders<GroupMember>.Update
                .Set(g => g.MemberStatus, "Left")
                .Set(g => g.LeftAt, DateTime.UtcNow);

            var result = await _GroupMembers.UpdateManyAsync(filter, update);

            return result.ModifiedCount;
        }


        public async Task<List<Account>> GetUsersInGroupAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return new List<Account>();

            var members = await _GroupMembers
                .Find(gm => gm.GroupId == groupId && gm.MemberStatus == "Accept")
                .ToListAsync();

            var accIds = members.Select(m => m.AccId).ToList();

            var usersInGroup = await _Accounts
                .Find(acc => accIds.Contains(acc.AccId))
                .ToListAsync();

            return usersInGroup;
        }
        public async Task<List<Account>> SearchUsersInGroupAsync(string groupId, string keyword)
        {
            if (!ObjectId.TryParse(groupId, out _)) return new List<Account>();

            var members = await _GroupMembers
                .Find(gm => gm.GroupId == groupId && gm.MemberStatus == "Accept")
                .ToListAsync();

            var accIds = members.Select(m => m.AccId).ToList();

            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.In(a => a.AccId, accIds) &
                         filterBuilder.Regex(a => a.FullName, new BsonRegularExpression(keyword, "i"));

            var matchedUsers = await _Accounts.Find(filter).ToListAsync();
            return matchedUsers;
        }

        public async Task<List<GroupMemberRequest>> GetJoinRequestsAsync(string groupId)
        {
            var filter = Builders<GroupMember>.Filter.Eq(gm => gm.GroupId, groupId) &
                         Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Pending");

            var groupMembers = await _GroupMembers.Find(filter).ToListAsync();

            var accIds = groupMembers.Select(m => m.AccId).ToList();

            var accountFilter = Builders<Account>.Filter.In(a => a.AccId, accIds);
            var accounts = await _Accounts.Find(accountFilter).ToListAsync();

            var joined = groupMembers.Join(accounts, m => m.AccId, a => a.AccId, (m, a) => new GroupMemberRequest
            {
                GroupMemberId = m.GroupMemberId,
                GroupId = m.GroupId,
                AccId = m.AccId,
                JointAt = m.JointAt,
                MemberStatus = m.MemberStatus,
                InviteByAccId = m.InviteByAccId,
                LeftAt = m.LeftAt,
                AccountFullName = a.FullName,
                AccountAvatar = a.Avatar ?? ""
            }).ToList();

            return joined;
        }


        public async Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId)
        {
            if (!ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(groupId, out _))
                return null;

            var existingMember = await _GroupMembers.Find(
                gm => gm.AccId == accId && gm.GroupId == groupId && gm.MemberStatus != "Left"
            ).FirstOrDefaultAsync();

            if (existingMember != null)
                return null; 

            var groupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                AccId = accId,
                GroupId = groupId,
                GroupRoleId = null, // chưa có vai trò khi chưa được duyệt
                MemberStatus = "Pending",
                JointAt = DateTime.UtcNow,
                InviteByAccId = null,
                LeftAt = null
            };

            await _GroupMembers.InsertOneAsync(groupMember);
            return groupMember;
        }
        public async Task<bool> RespondToJoinRequestAsync(string groupMemberId, string responseStatus)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return false;

            var validStatuses = new[] { "Accept", "Reject" };
            if (!validStatuses.Contains(responseStatus)) return false;

            var filter = Builders<GroupMember>.Filter.Eq(gm => gm.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Pending");

            var update = Builders<GroupMember>.Update
                .Set(gm => gm.MemberStatus, responseStatus)
                .Set(gm => gm.JointAt, DateTime.UtcNow);

            var result = await _GroupMembers.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
        public async Task<bool> UpdateRoleAsync(string groupId, string accId, string newGroupRoleId)
        {
            var filter = Builders<GroupMember>.Filter.Eq(m => m.GroupId, groupId) &
                         Builders<GroupMember>.Filter.Eq(m => m.AccId, accId) &
                         Builders<GroupMember>.Filter.Eq(m => m.MemberStatus, "Accept");

            var update = Builders<GroupMember>.Update.Set(m => m.GroupRoleId, newGroupRoleId);

            var result = await _GroupMembers.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

    }
}
