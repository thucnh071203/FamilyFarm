using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task Create(BookingService bookingService)
        {
            await _bookingService.InsertOneAsync(bookingService);
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
            
        }



    }
}
