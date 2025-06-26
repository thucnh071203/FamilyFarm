using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class BookingServiceByStatusDTO
    {
        public string BookingServiceId { get; set; } = null!;
        public string AccId { get; set; } = null!;
        public string ServiceId { get; set; } = null!;
        public decimal? Price { get; set; }
        public DateTime? BookingServiceAt { get; set; }
        public string? BookingServiceStatus { get; set; }
        public DateTime? CancelServiceAt { get; set; }
        public DateTime? RejectServiceAt { get; set; }
        public decimal? FirstPayment { get; set; }
        public DateTime? FirstPaymentAt { get; set; }
        public decimal? SecondPayment { get; set; }
        public DateTime? SecondPaymentAt { get; set; }
        public bool? IsDeleted { get; set; }

        // Thêm từ Service
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
    }
}
