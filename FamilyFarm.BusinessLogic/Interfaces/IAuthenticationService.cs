using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;

namespace FamilyFarm.BusinessLogic
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDTO?> Login(LoginRequestDTO request);
        Task<LoginResponseDTO?> ValidateRefreshToken(string refreshToken);

        //Param: access token from header
        //Return: Username
        string? GetDataFromToken(string accessToken);

        Task<RegisterFarmerResponseDTO?> RegisterFarmer(RegisterFarmerRequestDTO request);
    }
}
