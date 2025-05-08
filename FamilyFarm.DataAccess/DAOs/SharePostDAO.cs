using FamilyFarm.Models.Models;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class SharePostDAO
    {
        private readonly IMongoCollection<SharePost> _sharePosts;

        public SharePostDAO(IMongoDatabase database)
        {
            _sharePosts = database.GetCollection<SharePost>("SharePost");
        }

        public async Task<SharePost?> CreateAsync(SharePost? request)
        {
            if (request == null)
                return null;

            //Kiểm tra xem có Id hay chưa, nếu chưa thì tạo Id mới
            if (string.IsNullOrEmpty(request.PostId))
            {
                request.PostId = ObjectId.GenerateNewId().ToString();
            }

            request.CreatedAt = DateTime.UtcNow;

            await _sharePosts.InsertOneAsync(request);

            return request;
        }
    }
}
