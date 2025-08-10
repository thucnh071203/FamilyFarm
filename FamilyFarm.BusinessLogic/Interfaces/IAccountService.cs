using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        Task<Account?> GetAccountById(string acc_id);
        Task<Account?> GetAccountByUsername(string username);
        Task<Account> CreateAsync(Account account);
        Task<Account> UpdateAsync(string id, Account account);
        Task<Account> UpdateOtpAsync(string id, Account account);
        Task DeleteAsync(string id);
        Task<UpdateProfileResponseDTO> UpdateProfileAsync(string username, UpdateProfileRequestDTO account);
        Task<MyProfileDTO?> GetUserProfileAsync(string accId);
        Task<UpdateAvatarResponseDTO?> ChangeOwnAvatar(string? accountId, UpdateAvatarRequesDTO? request);
        Task<UpdateBackgroundResponseDTO?> ChangeOwnBackground(string? accountId, UpdateBackgroundRequestDTO? request);
        Task<Dictionary<string, (int Count, int Growth)>> GetTotalAndGrowthByRoleIdsAsync(List<string> roleIds);
        Task<TotalFarmerExpertDTO<Dictionary<string, int>>> GetUserGrowthOverTimeAsync(DateTime fromDate, DateTime toDate);
        Task<List<Account>?> GetAllAccountByRoleId(string role_id);
        Task<bool> UpdateAccountStatus(string accId, int status);
        Task<Account?> GetAccountByAccId(string accId);
        Task<List<Account>> GetAllAccountExceptAdmin();
        Task<ForgotPasswordResponseDTO> GetAccountByEmail(string email);
        Task<Account?> CheckAccountByEmail(string? email);
        Task<Account?> CheckAccountByPhone(string? phone);
        Task<Account?> UpdateCreditCard(string id, CreditCardUpdateRequestDTO requestCredit);
    }
}
