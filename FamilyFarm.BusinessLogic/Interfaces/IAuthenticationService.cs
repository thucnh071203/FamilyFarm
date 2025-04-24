using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;

namespace FamilyFarm.BusinessLogic
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDTO?> Login(LoginRequestDTO request);
        Task<LoginResponseDTO?> LoginWithGoogle(LoginGoogleRequestDTO request);
        Task<LoginResponseDTO?> ValidateRefreshToken(string refreshToken);

        Task<RegisterExpertReponseDTO?> RegisterExpert(RegisterExpertRequestDTO request);
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phone);
        bool IsValidIdentifierNumber(string identifierNumber);

        //Param: access token from header
        //Return: Username
        string? GetDataFromToken(string accessToken);


        Task<RegisterFarmerResponseDTO?> RegisterFarmer(RegisterFarmerRequestDTO request);

        Task<LoginResponseDTO> LoginFacebook(LoginFacebookRequestDTO request);

    }
}
