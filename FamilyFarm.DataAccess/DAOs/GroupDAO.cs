using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class GroupDAO
    {
        private readonly IMongoCollection<Group> _Groups;
        private readonly IMongoCollection<GroupMember> _GroupMembers;

        public GroupDAO(IMongoDatabase database)
        {
            _Groups = database.GetCollection<Group>("Group");
            _GroupMembers = database.GetCollection<GroupMember>("GroupMember");
        }

        public async Task<List<Group>> GetAllAsync()
        {
            return await _Groups.Find(g => g.IsDeleted != true).ToListAsync();
        }
        //public async Task<List<Group>> GetAllByUserId(string userId)
        //{
        //    //get list member by userId
        //    var memberFilter = Builders<GroupMember>.Filter.Eq(gm => gm.AccId, userId) &
        //               Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Accept");

        //    var memberList = await _GroupMembers.Find(memberFilter).ToListAsync();
        //    //Lấy tất cả các groupId duy nhất từ danh sách thành viên
        //    var groupIds = memberList.Select(m => m.GroupId).Distinct().ToList();
        //    //tìm group theo group id
        //    var groupFilter = Builders<Group>.Filter.In(a => a.GroupId, groupIds) &
        //        Builders<Group>.Filter.Ne(b => b.IsDeleted, true);// IsDeleted != true
        //    var groups = await _Groups.Find(groupFilter).ToListAsync();
        //    return groups;
        //}

        public async Task<List<GroupCardDTO>> GetAllByUserId(string userId)
        {
            // Lấy các groupId mà user đã tham gia (status = Accept)
            var memberFilter = Builders<GroupMember>.Filter.Eq(gm => gm.AccId, userId) &
                               Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Accept");

            var memberList = await _GroupMembers.Find(memberFilter).ToListAsync();
            var groupIds = memberList.Select(m => m.GroupId).Distinct().ToList();

            // Lấy các group hợp lệ
            var groupFilter = Builders<Group>.Filter.In(g => g.GroupId, groupIds) &
                              Builders<Group>.Filter.Ne(g => g.IsDeleted, true);
            var groups = await _Groups.Find(groupFilter).ToListAsync();

            // Trả về danh sách GroupCardDTO
            var result = new List<GroupCardDTO>();

            foreach (var group in groups)
            {
                var count = await _GroupMembers.CountDocumentsAsync(
                    Builders<GroupMember>.Filter.And(
                        Builders<GroupMember>.Filter.Eq(m => m.GroupId, group.GroupId),
                        Builders<GroupMember>.Filter.Eq(m => m.MemberStatus, "Accept")
                    )
                );

                result.Add(new GroupCardDTO
                {
                    group = group,
                    numberInGroup = (int)count
                });
            }

            return result;
        }

        public async Task<Group> GetByIdAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            return await _Groups.Find(g => g.GroupId == groupId && g.IsDeleted != true).FirstOrDefaultAsync();
        }

        public async Task<Group> CreateAsync(Group group)
        {
            group.GroupId = ObjectId.GenerateNewId().ToString();
            group.CreatedAt = DateTime.UtcNow;
            group.UpdatedAt = null;
            group.DeletedAt = null;
            group.IsDeleted = false;
            await _Groups.InsertOneAsync(group);
            return group;
        }

        public async Task<Group> UpdateAsync(string groupId, Group updateGroup)
        {
            if (!ObjectId.TryParse(groupId, out _)) return null;

            var filter = Builders<Group>.Filter.Eq(g => g.GroupId, groupId);

            if (filter == null) return null;

            var update = Builders<Group>.Update
                .Set(g => g.GroupName, updateGroup.GroupName)
                .Set(g => g.GroupAvatar, updateGroup.GroupAvatar)
                .Set(g => g.GroupBackground, updateGroup.GroupBackground)
                .Set(g => g.PrivacyType, updateGroup.PrivacyType)
                .Set(g => g.UpdatedAt, DateTime.UtcNow);

            var result = await _Groups.UpdateOneAsync(filter, update);

            var updatedGroup = await _Groups.Find(g => g.GroupId == groupId && g.IsDeleted != true).FirstOrDefaultAsync();

            return updatedGroup;
        }

        public async Task<long> DeleteAsync(string groupId)
        {
            if (!ObjectId.TryParse(groupId, out _)) return 0;

            var filter = Builders<Group>.Filter.Eq(g => g.GroupId, groupId);

            var update = Builders<Group>.Update
                .Set(g => g.DeletedAt, DateTime.UtcNow)
                .Set(g => g.IsDeleted, true);

            var result = await _Groups.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }

        public async Task<Group> GetLatestByCreatorAsync(string creatorId)
        {
            if (!ObjectId.TryParse(creatorId, out _)) return null;

            return await _Groups.Find(g => g.OwnerId == creatorId && g.IsDeleted != true)
                                .SortByDescending(g => g.CreatedAt)
                                .FirstOrDefaultAsync();
        }

        public async Task<List<GroupCardDTO>> GetGroupsSuggestion(string userId, int number)
        {
            // Lấy danh sách GroupId mà user đã tham gia
            var joinedGroupIds = await _GroupMembers
                .Find(gm => gm.AccId == userId && gm.MemberStatus == "Accept")
                .Project(gm => gm.GroupId)
                .ToListAsync();

            // Lọc các group chưa bị xóa và chưa được user tham gia
            var filter = Builders<Group>.Filter.Ne(g => g.IsDeleted, true) &
                         Builders<Group>.Filter.Nin(g => g.GroupId, joinedGroupIds);

            // Lấy danh sách group
            var groups = await _Groups.Find(filter)
                .Limit(number)
                .ToListAsync();

            // Tạo danh sách GroupCardDTO với số lượng thành viên
            var groupCardList = new List<GroupCardDTO>();

            foreach (var group in groups)
            {
                var memberCount = await _GroupMembers.CountDocumentsAsync(
     Builders<GroupMember>.Filter.And(
         Builders<GroupMember>.Filter.Eq(m => m.GroupId, group.GroupId),
         Builders<GroupMember>.Filter.Eq(m => m.MemberStatus, "Accept")
     )
 );

                groupCardList.Add(new GroupCardDTO
                {
                    group = group,
                    numberInGroup = (int)memberCount
                });
            }

            return groupCardList;
        }

        public async Task<List<string>> GetGroupIdsByUserId(string accId)
        {
            // Lấy danh sách group member của người dùng
            var filter = Builders<GroupMember>.Filter.Eq(gm => gm.AccId, accId) &
                         Builders<GroupMember>.Filter.Eq(gm => gm.MemberStatus, "Accept");

            var memberList = await _GroupMembers.Find(filter).ToListAsync();

            var groupIds = memberList
                .Select(m => m.GroupId)
                .Distinct()
                .ToList();

            if (!groupIds.Any())
                return new List<string>();

            // Kiểm tra các GroupId tương ứng có bị xóa hay không
            var groupFilter = Builders<Group>.Filter.In(g => g.GroupId, groupIds) &
                              Builders<Group>.Filter.Eq(g => g.IsDeleted, false);

            var validGroups = await _Groups.Find(groupFilter)
                                           .Project(g => g.GroupId) // chỉ lấy GroupId
                                           .ToListAsync();

            return validGroups;
        }

        public async Task<List<GroupCardDTO>> SearchGroups(string userId, string searchTerm)
        {
            // Tạo filter để tìm kiếm group theo tên hoặc mô tả
            var searchFilter = Builders<Group>.Filter.Or(
                Builders<Group>.Filter.Regex(g => g.GroupName, new BsonRegularExpression(searchTerm, "i"))
            );

            // Lọc các group chưa bị xóa và khớp với từ khóa tìm kiếm
            var filter = Builders<Group>.Filter.Ne(g => g.IsDeleted, true) &
                         searchFilter;

            // Lấy danh sách group
            var groups = await _Groups.Find(filter)
                .ToListAsync();

            // Tạo danh sách GroupCardDTO với số lượng thành viên
            var groupCardList = new List<GroupCardDTO>();
            foreach (var group in groups)
            {
                var memberCount = await _GroupMembers.CountDocumentsAsync(
                    Builders<GroupMember>.Filter.And(
                        Builders<GroupMember>.Filter.Eq(m => m.GroupId, group.GroupId),
                        Builders<GroupMember>.Filter.Eq(m => m.MemberStatus, "Accept")
                    )
                );

                groupCardList.Add(new GroupCardDTO
                {
                    group = group,
                    numberInGroup = (int)memberCount
                });
            }

            return groupCardList;
        }
    }
}
