using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class BillPaymentMapper
    {
        // PaymentTransaction
        public string? PaymentId { get; set; }
        public string? BookingServiceId { get; set; }
        public string? SubProcessId { get; set; }
        public string? FromAccId { get; set; }
        public string? ToAccId { get; set; }
        public bool? IsRepayment { get; set; }
        public DateTime? PayAt { get; set; }
        public decimal? Price { get; set; }

        // Service Information
        public string? ServiceName { get; set; }

        // Service category information
        public string? CategoryServiceName { get; set; }

        // User Information
        public string? ExpertName { get; set; }
        public string? FarmerName { get; set; }
        public string? PayerName { get; set; }

        // Booking information
        public DateTime? BookingServiceAt { get; set; }
    }
}
