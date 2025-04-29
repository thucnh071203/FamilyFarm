using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class GroupMemberDAO
    {
        private readonly IMongoCollection<GroupMember> _GroupMembers;

        public GroupMemberDAO(IMongoDatabase database)
        {
            _GroupMembers = database.GetCollection<GroupMember>("GroupMember");
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
    }
}
