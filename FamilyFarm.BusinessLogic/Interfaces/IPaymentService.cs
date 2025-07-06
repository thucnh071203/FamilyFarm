using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using Microsoft.AspNetCore.Http;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> GetAllPayment();
        Task<PaymentTransaction> GetPayment(string id);
        Task<PaymentTransaction> CreatePayment(PaymentTransaction transaction);
        Task<bool> HandleVNPayReturnAsync(IQueryCollection vnpayData);
        Task<string> CreatePaymentUrlAsync(CreatePaymentRequestDTO request, HttpContext httpContext);
        Task<PaymentResponseDTO> GetPaymentByBooking(string bookingId);
        Task<PaymentResponseDTO> GetPaymentBySubProcess(string processId);
    }
}
