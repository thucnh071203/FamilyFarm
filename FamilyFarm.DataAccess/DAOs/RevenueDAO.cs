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
    public class RevenueDAO
    {
        private readonly IMongoCollection<Revenue> _Revenue;
        public RevenueDAO(IMongoDatabase database)
        {
            _Revenue = database.GetCollection<Revenue>("Revenue");
        }

        /// <summary>
        /// Lấy revenue duy nhất trong hệ thống (nếu chưa có thì tạo mới)
        /// </summary>
        public async Task<Revenue> GetOrCreateRevenueAsync()
        {
            var revenue = await _Revenue.Find(_ => true).FirstOrDefaultAsync();
            if (revenue == null)
            {
                revenue = new Revenue
                {
                    RevenueId = ObjectId.GenerateNewId().ToString(),
                    TotalRevenue = 0,
                    CommissionRevenue = 0
                };
                await _Revenue.InsertOneAsync(revenue);
            }

            return revenue;
        }

        public async Task<bool> ChangeRevenueAsync(decimal? totalAmount, decimal? commission)
        {
            if (totalAmount == null || commission == null) return false;

            // Chỉ cộng đúng một lần
            var update = Builders<Revenue>.Update
                .Inc(r => r.TotalRevenue, totalAmount.Value)
                .Inc(r => r.CommissionRevenue, commission.Value);

            var result = await _Revenue.UpdateOneAsync(
                Builders<Revenue>.Filter.Empty,
                update
            );

            return result.ModifiedCount > 0;
        }


        //public async Task<bool> ChangeRevenueAsync(decimal? totalAmount, decimal? commission)
        //{
        //    var filter = Builders<Revenue>.Filter.Empty;

        //    var update = Builders<Revenue>.Update
        //        .Inc(r => r.TotalRevenue, totalAmount)
        //        .Inc(r => r.CommissionRevenue, commission);

        //    var result = await _Revenue.UpdateOneAsync(filter, update);
        //    return result.ModifiedCount > 0;
        //}

        /// <summary>
        /// Tăng tổng doanh thu và hoa hồng
        /// </summary>
        //public async Task<bool> IncreaseRevenueAsync(decimal totalAmount, decimal commission)
        //{
        //    var filter = Builders<Revenue>.Filter.Empty;

        //    var update = Builders<Revenue>.Update
        //        .Inc(r => r.TotalRevenue, totalAmount)
        //        .Inc(r => r.CommissionRevenue, commission);

        //    var result = await _Revenue.UpdateOneAsync(filter, update);
        //    return result.ModifiedCount > 0;
        //}

        /// <summary>
        /// Trừ doanh thu (ví dụ khi hoàn tiền)
        /// </summary>
        //public async Task<bool> DecreaseRevenueAsync(decimal totalAmount, decimal commission)
        //{
        //    var filter = Builders<Revenue>.Filter.Empty;

        //    var update = Builders<Revenue>.Update
        //        .Inc(r => r.TotalRevenue, -totalAmount)
        //        .Inc(r => r.CommissionRevenue, -commission);

        //    var result = await _Revenue.UpdateOneAsync(filter, update);
        //    return result.ModifiedCount > 0;
        //}

        /// <summary>
        /// Lấy thông tin doanh thu
        /// </summary>
        public async Task<Revenue?> GetRevenueAsync()
        {
            return await _Revenue.Find(_ => true).FirstOrDefaultAsync();
        }
    }
}
