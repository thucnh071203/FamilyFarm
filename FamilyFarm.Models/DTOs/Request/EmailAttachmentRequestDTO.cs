using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Request
{
    public class EmailAttachmentRequestDTO
    {
        public string ToAccId { get; set; }
        public string Subject { get; set; }
        public string PaymentId { get; set; } // 👈 thêm nếu cần truy bill
    }
}
