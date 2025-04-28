using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface IBookingServiceService
    {
        Task<BookingService?> GetById(string id);
        Task<List<BookingServiceResponseDTO>?> GetAllBookingOfExpert(string id);
        Task<List<BookingServiceResponseDTO>?> GetAllBookingOfFarmer(string id);
        Task<List<BookingServiceResponseDTO>?> GetRequestBookingOfExpert(string id);
        Task<List<BookingServiceResponseDTO>?> GetRequestBookingOfFarmer(string id);
        Task<List<BookingServiceResponseDTO>?> ExpertResponseBookingService (string id);
        Task<List<BookingService>?> CancelBookingService(string id);
        Task<bool?> SendRequestBooking(string username, string serviceId);
    }
}
