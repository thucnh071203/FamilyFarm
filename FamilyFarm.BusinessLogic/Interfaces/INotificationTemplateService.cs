using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface INotificationTemplateService
    {
        string GetNotificationTemplate(string categoryName);
    }
}
