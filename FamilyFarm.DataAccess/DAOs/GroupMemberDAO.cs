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
            groupMember.GroupRoleId = "680cebdfac700e1cb4c165b2";
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

        public async Task<GroupMember> DeleteAsync(string groupMemberId)
        {
            if (!ObjectId.TryParse(groupMemberId, out _)) return null;

            var filter = Builders<GroupMember>.Filter.Eq(g => g.GroupMemberId, groupMemberId) &
                         Builders<GroupMember>.Filter.Eq(g => g.MemberStatus, "Accept");

            if (filter == null) return null;

            var update = Builders<GroupMember>.Update
                .Set(g => g.MemberStatus, "Left")
                .Set(g => g.LeftAt, DateTime.UtcNow);

            var result = await _GroupMembers.UpdateOneAsync(filter, update);

            if (result.MatchedCount > 0 && result.ModifiedCount > 0)
            {
                return await GetByIdAsync(groupMemberId);
            }

            return null;
        }
    }
}
