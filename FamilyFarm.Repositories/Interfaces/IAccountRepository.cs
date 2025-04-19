using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Repositories
{
    public interface IAccountRepository
    {
        //Get all account
        //role_id: 1 is ADMIN, 2 is FARMER, 3 is EXPERT
        //status: 0 is ACTIVED, 1 is DELETED, 2 is LOCK, 3 if get all
        Task<List<Account>> GetAll(string role_id, int status);

        //Get by Account Id (not check status)
        Task<Account> GetAccountById(string acc_id);

        //Get by Username (not check status)
        Task<Account> GetAccountByUsername(string username);

        //Get by Email
        Task<Account> GetAccountByEmail(string email);

        //Get by Phone
        Task<Account> GetAccountByPhone(string phone);

        //Get by Identifier: username, email, and phone
        Task<Account?> GetAccountByIdentifier(string identifier);
    }
}
