﻿using System;
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
        Task<TotalFarmerExpertDTO<Dictionary<string, int>>> GetTotalByRoleIdsAsync(List<string> roleIds);
        Task<TotalFarmerExpertDTO<Dictionary<string, int>>> GetUserGrowthOverTimeAsync(DateTime fromDate, DateTime toDate);
    }
}
