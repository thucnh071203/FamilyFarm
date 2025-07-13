using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.Response
{
    public class ListPaymentResponseDTO
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<PaymentDataMapper> Data { get; set; }
    }
}
