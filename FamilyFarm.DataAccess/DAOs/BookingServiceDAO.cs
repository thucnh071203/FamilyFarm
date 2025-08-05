using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class BookingServiceDAO:SingletonBase
    {
        private readonly IMongoCollection<BookingService> _bookingService;
        public BookingServiceDAO(IMongoDatabase database)
        {
            _bookingService = database.GetCollection<BookingService>("BookingService");
        }

        public async Task<BookingService?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.BookingServiceId, id),
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false)
            );

            return await _bookingService.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<BookingService>?> GetAllBookingComplete()
        {
            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false),
                Builders<BookingService>.Filter.Eq(c => c.IsCompletedFinal, true)
            //Builders<BookingService>.Filter.Eq(c => c.BookingServiceStatus, "Completed")
            );

            return await _bookingService.Find(filter).ToListAsync();
        }

        public async Task<List<BookingService>?> GetAllBookingByAccid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.AccId, id),
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false)
            );

            return await _bookingService.Find(filter).ToListAsync();
        }

        public async Task<List<BookingService>?> GetListRequestBookingByAccid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.AccId, id),
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false),
                 Builders<BookingService>.Filter.Eq(c => c.BookingServiceStatus, "Pending")
            );

            return await _bookingService.Find(filter).ToListAsync();
        }

        public async Task<List<BookingService>?> GetAllBookingByServiceId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.ServiceId, id),
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false)
            );

            return await _bookingService.Find(filter).ToListAsync();
        }

        public async Task<List<BookingService>?> GetListRequestBookingByServiceId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(c => c.ServiceId, id),
                Builders<BookingService>.Filter.Eq(c => c.IsDeleted, false),
                 Builders<BookingService>.Filter.Eq(c => c.BookingServiceStatus, "Pending")
            );

            return await _bookingService.Find(filter).ToListAsync();
        }

        public async Task<bool> Create(BookingService bookingService)
        {
            try
            {
                await _bookingService.InsertOneAsync(bookingService);
                return true;
            }
            catch (Exception)
            {
                // Có thể log lỗi ra tại đây nếu cần
                return false;
            }
        }

        public async Task UpdateStatus(BookingService bookingService)
        {
            var filter = Builders<BookingService>.Filter.Eq(a => a.BookingServiceId, bookingService.BookingServiceId);
            if (bookingService.BookingServiceStatus.Equals("Cancel"))
            {
                var update = Builders<BookingService>.Update
                        .Set(a => a.BookingServiceStatus, bookingService.BookingServiceStatus)
                        .Set(a => a.CancelServiceAt, bookingService.CancelServiceAt);
                await _bookingService.UpdateOneAsync(filter, update);
            }else if (bookingService.BookingServiceStatus.Equals("Reject"))
            {
                var update = Builders<BookingService>.Update
                       .Set(a => a.BookingServiceStatus, bookingService.BookingServiceStatus)
                       .Set(a => a.RejectServiceAt, bookingService.RejectServiceAt);
                await _bookingService.UpdateOneAsync(filter, update);
            }else if (bookingService.BookingServiceStatus.Equals("Accepted"))
            {
                var update = Builders<BookingService>.Update
                      .Set(a => a.BookingServiceStatus, bookingService.BookingServiceStatus);
                      
                await _bookingService.UpdateOneAsync(filter, update);
            }
            else if (bookingService.BookingServiceStatus.Equals("Completed"))
            {
                var update = Builders<BookingService>.Update
                      .Set(a => a.BookingServiceStatus, bookingService.BookingServiceStatus)
                      .Set(a => a.IsCompletedFinal, bookingService.IsCompletedFinal);

                await _bookingService.UpdateOneAsync(filter, update);
            }

        }

        public async Task<bool?> UpdateStatus(string? bookingId, string? status)
        {
            if (string.IsNullOrEmpty(bookingId) || string.IsNullOrEmpty(status))
                return null;

            var filter = Builders<BookingService>.Filter.Eq(a => a.BookingServiceId, bookingId);

            var update = Builders<BookingService>.Update.Set(a => a.BookingServiceStatus, status);

            var result = await _bookingService.UpdateOneAsync(filter, update);

            return result.MatchedCount > 0;
        }

        public async Task<List<BookingService>?> GetBookingByExpert(string? expertId, string? status)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var filterBuilder = Builders<BookingService>.Filter;

            // Tạo filter theo expertId
            var filter = filterBuilder.Eq(b => b.ExpertId, expertId);

            // Nếu có status thì thêm điều kiện lọc theo status
            if (!string.IsNullOrEmpty(status))
            {
                var statusFilter = filterBuilder.Eq(b => b.BookingServiceStatus, status);
                filter = filter & statusFilter;
            }

            var results = await _bookingService.Find(filter)
                .SortByDescending(b => b.BookingServiceAt)
                .ToListAsync();
            return results;
        }

        public async Task<List<BookingService>> GetListExtraRequest(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return new List<BookingService>();

            var filter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(b => b.ExpertId, expertId),
                Builders<BookingService>.Filter.Eq(b => b.BookingServiceStatus, "Extra Request"),
                Builders<BookingService>.Filter.Eq(b => b.HasExtraProcess, false)
            );

            var result = await _bookingService.Find(filter).ToListAsync();
            return result;
        }

        public async Task<BookingService> UpdateBookingPaymentAsync(string bookingId, BookingService booking)
        {
            if (!ObjectId.TryParse(bookingId, out _)) return null;


            var filter = Builders<BookingService>.Filter.Eq(g => g.BookingServiceId, bookingId);

            if (filter == null) return null;

            var update = Builders<BookingService>.Update
                .Set(g => g.IsPaidByFarmer, booking.IsPaidByFarmer)
                .Set(g => g.IsPaidToExpert, booking.IsPaidToExpert)
                .Set(g => g.BookingServiceStatus, booking.BookingServiceStatus);

            var result = await _bookingService.UpdateOneAsync(filter, update);

            var updatedBookingPay = await _bookingService.Find(g => g.BookingServiceId == bookingId && g.IsDeleted != true).FirstOrDefaultAsync();

            return updatedBookingPay;
        }

        public async Task<BookingService?> UpdateBookingAsync(string bookingServiceId, BookingService updatedBookingService)
        {
            if (string.IsNullOrEmpty(bookingServiceId))
                return null;

            // Kiểm tra xem bookingServiceId có hợp lệ hay không
            if (!ObjectId.TryParse(bookingServiceId, out _))
                return null;

            var filter = Builders<BookingService>.Filter.Eq(b => b.BookingServiceId, bookingServiceId);

            // Tạo bản cập nhật các trường mà bạn muốn thay đổi
            var update = Builders<BookingService>.Update
                .Set(b => b.ServiceName, updatedBookingService.ServiceName)
                .Set(b => b.Description, updatedBookingService.Description)
                .Set(b => b.Price, updatedBookingService.Price)
                .Set(b => b.CommissionRate, updatedBookingService.CommissionRate)
                .Set(b => b.BookingServiceAt, updatedBookingService.BookingServiceAt)
                .Set(b => b.PaymentDueDate, updatedBookingService.PaymentDueDate)
                .Set(b => b.BookingServiceStatus, updatedBookingService.BookingServiceStatus)
                .Set(b => b.IsPaidByFarmer, updatedBookingService.IsPaidByFarmer)
                .Set(b => b.IsPaidToExpert, updatedBookingService.IsPaidToExpert)
                .Set(b => b.IsCompletedFinal, updatedBookingService.IsCompletedFinal)
                .Set(b => b.CancelServiceAt, updatedBookingService.CancelServiceAt)
                .Set(b => b.RejectServiceAt, updatedBookingService.RejectServiceAt)
                .Set(b => b.IsDeleted, updatedBookingService.IsDeleted)
                .Set(b => b.ExpertId, updatedBookingService.ExpertId)
                .Set(b => b.ServiceId, updatedBookingService.ServiceId)
                .Set(b => b.ExtraDescription, updatedBookingService.ExtraDescription)
                .Set(b => b.HasExtraProcess, updatedBookingService.HasExtraProcess);

            // Thực hiện cập nhật
            var result = await _bookingService.UpdateOneAsync(filter, update);

            // Kiểm tra nếu có bản ghi được sửa đổi
            if (result.ModifiedCount > 0)
            {
                // Trả về bản ghi đã được cập nhật
                return await _bookingService.Find(b => b.BookingServiceId == bookingServiceId).FirstOrDefaultAsync();
            }

            return null; // Nếu không có gì thay đổi, trả về null
        }

        public async Task<BookingService?> GetLastestBookingByFarmerAsync(string? farmerId)
        {
            if (string.IsNullOrEmpty(farmerId))
                return null;

            // Kiểm tra xem bookingServiceId có hợp lệ hay không
            if (!ObjectId.TryParse(farmerId, out _))
                return null;

            var filter = Builders<BookingService>.Filter.Eq(x => x.AccId, farmerId) &
                         Builders<BookingService>.Filter.Eq(x => x.IsDeleted, false);

            var sort = Builders<BookingService>.Sort.Descending(x => x.BookingServiceAt);

            return await _bookingService.Find(filter)
                            .Sort(sort)
                            .FirstOrDefaultAsync();
        }
    }
}
