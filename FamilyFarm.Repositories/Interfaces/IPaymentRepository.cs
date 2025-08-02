using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<List<PaymentTransaction>> GetAllPayment();
        Task<PaymentTransaction> GetPayment(string id);
        Task<PaymentTransaction> CreatePayment(PaymentTransaction transaction);
        Task<PaymentTransaction> GetPaymentByBooking(string bookingId);
        Task<PaymentTransaction> GetPaymentBySubProcess(string subProcessId);
        Task<PaymentTransaction> GetRepaymentByBookingId(string bookingId);
        Task<PaymentTransaction> GetRepaymentBySubProcessId(string subProcessId);
        Task<PaymentTransaction> GetLatestPaymentOfPayerById(string fromAccId);
    }
}
