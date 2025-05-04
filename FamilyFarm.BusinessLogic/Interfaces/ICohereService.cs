using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ICohereService
    {
        Task<bool> IsAgricultureRelatedAsync(string content);
    }
}
