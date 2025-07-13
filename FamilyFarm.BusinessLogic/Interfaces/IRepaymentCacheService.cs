using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IRepaymentCacheService
    {
        void SaveAdminId(string txnRef, string adminId);
        string? GetAdminId(string txnRef);
        void RemoveAdminId(string txnRef);
    }
}
