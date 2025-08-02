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
    public class PaymentDAO
    {
        private readonly IMongoCollection<PaymentTransaction> _Payments;

        public PaymentDAO(IMongoDatabase database)
        {
            _Payments = database.GetCollection<PaymentTransaction>("Payment");
        }

        public async Task<List<PaymentTransaction>> GetAllAsync()
        {
            return await _Payments.Find(_ => true).ToListAsync();
        }

        public async Task<PaymentTransaction> GetByIdAsync(string paymentId)
        {
            if (!ObjectId.TryParse(paymentId, out _)) return null;

            return await _Payments.Find(p => p.PaymentId == paymentId).FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction> GetByBookingIdAsync(string bookingId)
        {
            if (!ObjectId.TryParse(bookingId, out _)) return null;

            return await _Payments
                .Find(p => p.BookingServiceId == bookingId && p.SubProcessId == null)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction> GetBySubProcessIdAsync(string subProcessId)
        {
            if (!ObjectId.TryParse(subProcessId, out _)) return null;

            return await _Payments.Find(p => p.SubProcessId == subProcessId).FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction payment)
        {
            await _Payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task<PaymentTransaction> GetRepaymentByBookingIdAsync(string bookingId)
        {
            if (!ObjectId.TryParse(bookingId, out _)) return null;

            return await _Payments
                .Find(p => p.BookingServiceId == bookingId && p.SubProcessId == null && p.IsRepayment == true)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction> GetRepaymentBySubProcessIdAsync(string subProcessId)
        {
            if (!ObjectId.TryParse(subProcessId, out _)) return null;

            return await _Payments.Find(p => p.SubProcessId == subProcessId && p.IsRepayment == true).FirstOrDefaultAsync();
        }

        public async Task<PaymentTransaction?> GetLatestOfPayerByIdAsync(string fromAccId)
        {
            if (string.IsNullOrWhiteSpace(fromAccId) || !ObjectId.TryParse(fromAccId, out _))
                return null;

            var filter = Builders<PaymentTransaction>.Filter.Eq(p => p.FromAccId, fromAccId);

            return await _Payments.Find(filter)
                                   .SortByDescending(p => p.PayAt) // nếu muốn lấy bản mới nhất
                                   .FirstOrDefaultAsync();
        }
    }
}
