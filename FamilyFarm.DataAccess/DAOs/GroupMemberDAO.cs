using System;
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
using Group = FamilyFarm.Models.Models.Group;

namespace FamilyFarm.DataAccess.DAOs
{
    public class GroupMemberDAO
    {
        private readonly IMongoCollection<GroupMember> _GroupMembers;
        private readonly IMongoCollection<Account> _Accounts;
        private readonly IMongoCollection<Group> _Group;
        public GroupMemberDAO(IMongoDatabase database)
        {
            _GroupMembers = database.GetCollection<GroupMember>("GroupMember");
            _Accounts = database.GetCollection<Account>("Account");
            _Group = database.GetCollection<Group>("Group");
        }


        public async Task<GroupMember> GetByIdAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return null;

            return await _GroupMembers.Find(g => g.GroupMemberId == groupMemberId && g.MemberStatus.Equals("Accept")).FirstOrDefaultAsync();
        }
        

        public async Task<GroupMember> AddAsync(string groupId, string accountId, string inviterId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            if (!ObjectId.TryParse(accountId, out _)) return null;

            var addGroupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                GroupRoleId = "680cebdfac700e1cb4c165b2", // mặc định là member
                GroupId = groupId,
                AccId = accountId,
                JointAt = DateTime.Now,
                MemberStatus = "Accept",
                InviteByAccId = inviterId,
                LeftAt = null
            };


            await _GroupMembers.InsertOneAsync(addGroupMember);

            return addGroupMember;
        }

        public async Task<GroupMember> AddOwnersync(string groupId, string accountId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            if (!ObjectId.TryParse(accountId, out _)) return null;

            var addGroupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                GroupRoleId = "680ce8722b3eec497a30201e", // mặc định là owner
                GroupId = groupId,
                AccId = accountId,
                JointAt = DateTime.Now,
                MemberStatus = "Accept",
                InviteByAccId = null,
                LeftAt = null
            };


            await _GroupMembers.InsertOneAsync(addGroupMember);

            return addGroupMember;
        }

        public async Task<long> DeleteAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return 0;

            var filter = Builders<GroupMember>.Filter.Eq(g => g.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept");

            if (filter == null) return 0;

            var deleteResult = await _GroupMembers.DeleteOneAsync(filter);
            return deleteResult.DeletedCount;
        }

        public async Task<long> DeleteAllAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return -1;

            var filter = Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId) &
                         Builders<GroupMember>.Filter.In(g => g.MemberStatus, new[] { "Accept", "Pending" });


            var update = Builders<GroupMember>.Update
                .Set(g => g.MemberStatus, "Left")
                .Set(g => g.LeftAt, DateTime.UtcNow);

            var result = await _GroupMembers.UpdateManyAsync(filter, update);

            return result.ModifiedCount;
        }



        //public async Task<List<Account>> GetUsersInGroupAsync(string groupId)
        //{
        //    if (!ObjectId.TryParse(groupId, out _)) return new List<Account>();

        //    var members = await _GroupMembers
        //        .Find(gm => gm.GroupId == groupId && gm.MemberStatus == "Accept")
        //        .ToListAsync();

        //    var accIds = members.Select(m => m.AccId).ToList();

        //    var usersInGroup = await _Accounts
        //        .Find(acc => accIds.Contains(acc.AccId))
        //        .ToListAsync();

        //    return usersInGroup;
        //}

        public async Task<List<GroupMemberResponseDTO>> GetUsersInGroupAsync(string groupId)
        {
            var filter = Builders<GroupMember>.Filter.Eq(gm => gm.GroupId, groupId) &
                         Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Accept");

            var groupMembers = await _GroupMembers.Find(filter).ToListAsync();


            var accIds = groupMembers.Select(m => m.AccId).ToList();

            var accountFilter = Builders<Account>.Filter.In(a => a.AccId, accIds);
            var accounts = await _Accounts.Find(accountFilter).ToListAsync();


            var joined = groupMembers.Join(accounts, m => m.AccId, a => a.AccId, (m, a) => new GroupMemberResponseDTO
            {
                GroupMemberId = m.GroupMemberId,
                GroupId = m.GroupId,
                AccId = m.AccId,
                JointAt = m.JointAt,
                MemberStatus = m.MemberStatus,
                FullName = a.FullName,
                Avatar = a.Avatar ?? "",
                City = a.City,
                RoleInGroupId = m.GroupRoleId,
            }).ToList();

            return joined;

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
                AccountAvatar = a.Avatar ?? "",
                City = a.City
            }).ToList();

            return joined;
        }


        public async Task<GroupMember> RequestToJoinGroupAsync(string accId, string groupId)
        {
            if (!ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(groupId, out _))
                return null;

            // Kiểm tra người này đã trong group chưa
            var existingMember = await _GroupMembers.Find(
                gm => gm.AccId == accId && gm.GroupId == groupId && gm.MemberStatus != "Left"
            ).FirstOrDefaultAsync();

            if (existingMember != null)
                return null;

            // Lấy thông tin group
            var group = await _Group.Find(g => g.GroupId == groupId).FirstOrDefaultAsync();
            if (group == null)
                return null;

            // Tùy vào PrivacyType mà set role và status
            string roleId = null;
            string status = "Pending";

            if (group.PrivacyType == "Public")
            {
                roleId = "680cebdfac700e1cb4c165b2"; // ID vai trò Member
                status = "Accept";
            }

            var groupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                AccId = accId,
                GroupId = groupId,
                GroupRoleId = roleId,
                MemberStatus = status,
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
            if (responseStatus.Equals(validStatuses[0]))
            {
                var update = Builders<GroupMember>.Update
                    .Set(gm => gm.GroupRoleId, "680cebdfac700e1cb4c165b2")
                .Set(gm => gm.MemberStatus, responseStatus)
                .Set(gm => gm.JointAt, DateTime.UtcNow);
                var result = await _GroupMembers.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            else
            {
                var deleteResult = await _GroupMembers.DeleteOneAsync(filter);
                return deleteResult.DeletedCount > 0;
            }


        }
        public async Task<bool> UpdateRoleAsync(string groupMemberId, string newGroupRoleId)
        {
            var filter = Builders<GroupMember>.Filter.Eq(m => m.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(m => m.MemberStatus, "Accept");

            var update = Builders<GroupMember>.Update.Set(m => m.GroupRoleId, newGroupRoleId);

            var result = await _GroupMembers.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> LeaveGroupAsync(string groupId, string accId)
        {
            // Kiểm tra nếu cần (nếu bạn lưu groupId và accId dưới dạng ObjectId)
            if (!ObjectId.TryParse(groupId, out _) || !ObjectId.TryParse(accId, out _))
                return false;

            // Tìm GroupMember hợp lệ
            var filter = Builders<GroupMember>.Filter.And(
                Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId),
                Builders<GroupMember>.Filter.Eq(g => g.AccId, accId),
                Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept")
            );

            var groupMember = await _GroupMembers.Find(filter).FirstOrDefaultAsync();
            if (groupMember == null) return false;

            // Xóa nếu tìm thấy
            var result = await _GroupMembers.DeleteOneAsync(g => g.GroupMemberId == groupMember.GroupMemberId);
            return result.DeletedCount > 0;
        }

        public async Task<GroupMember> GetMemberJoinedGroupAsync(string groupId, string accId)
        {
            if (!ObjectId.TryParse(groupId, out _) || !ObjectId.TryParse(accId, out _))
                return null;

            // Tìm GroupMember hợp lệ
            var filter = Builders<GroupMember>.Filter.And(
                Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId),
                Builders<GroupMember>.Filter.Eq(g => g.AccId, accId),
                Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept")
            );

            return await _GroupMembers.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<GroupMember> InviteAsync(string groupId, string accountId, string inviterId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            if (!ObjectId.TryParse(accountId, out _)) return null;

            if (!ObjectId.TryParse(accountId, out _)) return null;

            var addGroupMember = new GroupMember
            {
                GroupMemberId = ObjectId.GenerateNewId().ToString(),
                GroupRoleId = "680cebdfac700e1cb4c165b2", // mặc định là member
                GroupId = groupId,
                AccId = accountId,
                JointAt = DateTime.Now,
                MemberStatus = "Invite",
                InviteByAccId = inviterId,
                LeftAt = null
            };


            await _GroupMembers.InsertOneAsync(addGroupMember);

            return addGroupMember;
        }

        public async Task<bool> RespondToInviteRequestAsync(string groupMemberId, string responseStatus)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return false;

            var validStatuses = new[] { "Accept", "Pending", "Reject" };
            if (!validStatuses.Contains(responseStatus)) return false;

            var filter = Builders<GroupMember>.Filter.Eq(gm => gm.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Invite");
            if (responseStatus.Equals(validStatuses[0]) || responseStatus.Equals(validStatuses[1]))
            {
                var update = Builders<GroupMember>.Update
                    .Set(gm => gm.GroupRoleId, "680cebdfac700e1cb4c165b2")
                .Set(gm => gm.MemberStatus, responseStatus)
                .Set(gm => gm.JointAt, DateTime.UtcNow);
                var result = await _GroupMembers.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            else
            {
                var deleteResult = await _GroupMembers.DeleteOneAsync(filter);
                return deleteResult.DeletedCount > 0;
            }
        }

        public async Task<GroupMember> GetMemberInvitedGroupAsync(string groupId, string accId)
        {
            if (!ObjectId.TryParse(groupId, out _) || !ObjectId.TryParse(accId, out _))
                return null;

            // Tìm GroupMember hợp lệ
            var filter = Builders<GroupMember>.Filter.And(
                Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId),
                Builders<GroupMember>.Filter.Eq(g => g.AccId, accId),
                Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Invite")
            );

            return await _GroupMembers.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<GroupMember> GetMemberInvitedOrJoinedGroupAsync(string groupId, string accId)
        {
            if (!ObjectId.TryParse(groupId, out _) || !ObjectId.TryParse(accId, out _))
                return null;

            // Tìm GroupMember hợp lệ
            var filter = Builders<GroupMember>.Filter.And(
                Builders<GroupMember>.Filter.Eq(g => g.GroupId, groupId),
                Builders<GroupMember>.Filter.Eq(g => g.AccId, accId),
                Builders<GroupMember>.Filter.In(g => g.MemberStatus, new[] { "Invite", "Accept", "Pending" })
            );

            return await _GroupMembers.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<GroupMember> GetMemberInviteByIdAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return null;

            return await _GroupMembers.Find(g => g.GroupMemberId == groupMemberId && g.MemberStatus.Equals("Invite")).FirstOrDefaultAsync();
        }
    }
}
