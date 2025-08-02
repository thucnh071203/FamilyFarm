using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateBillPdf(BillPaymentMapper data);
    }
}
