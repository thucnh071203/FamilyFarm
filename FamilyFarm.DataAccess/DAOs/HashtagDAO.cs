using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class HashtagDAO
    {
        private readonly IMongoCollection<HashTag> _hashtagCollection;

        public HashtagDAO(IMongoDatabase database)
        {
            _hashtagCollection = database.GetCollection<HashTag>("HashTag");
        }

        /// <summary>
        ///     Create hash tag
        /// </summary>
        public async Task<HashTag?> CreateHashTag(HashTag? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            await _hashtagCollection.InsertOneAsync(request);

            return request;
        }

        /// <summary>
        ///     Get list hashtag of post id
        /// </summary>
        public async Task<List<HashTag>?> GetAllHashTagOfPost(string? post_id)
        {
            if (string.IsNullOrEmpty(post_id))
                return null;

            var result = await _hashtagCollection
                .Find(pc => pc.PostId == post_id)
                .ToListAsync();

            return result;
        }
    }
}
