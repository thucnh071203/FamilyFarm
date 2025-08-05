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
        Task<PaymentResponseDTO> GetRePaymentByBooking(string bookingId);
        Task<PaymentResponseDTO> GetPaymentBySubProcess(string processId);
        Task<PaymentResponseDTO> GetRePaymentBySubprocess(string subprocessId);
        //Task<RepaymentResponseDTO> RepayToExpertAsync(CreateRepaymentRequestDTO);
        Task<bool> HandleRepaymentVNPayReturnAsync(IQueryCollection vnpayData);
        Task<string> CreateRepaymentUrlAsync(CreateRepaymentRequestDTO request, HttpContext httpContext);
        Task<ListPaymentResponseDTO> GetListPayment();
        Task<BillPaymentResponseDTO> GetBillPayment(string? paymentId);
        Task<PaymentTransaction> GetLatestPaymentOfPayerById(string fromAccId);
        Task<BillPaymentResponseDTO> GetBillPaymentByBookingId(string? bookingId);
        Task<ListPaymentResponseDTO> GetListPaymentUser(string accId);
    }
}
