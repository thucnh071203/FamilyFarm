using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    interface IAccountService
    {
        Task<Account> getAccountById(string account_id, int status);
        Task<Account> getAccount(string username, int status);
    }
}
