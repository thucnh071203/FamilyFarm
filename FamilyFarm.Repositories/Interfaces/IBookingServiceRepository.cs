using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Interfaces
{
    public interface IBookingServiceRepository
    {
       Task<BookingService?> GetById(string id);
        Task<List<BookingService>?> GetAllBookingByAccid(string id);
        Task<List<BookingService>?> GetListRequestBookingByAccid(string id);
        Task<List<BookingService>?> GetAllBookingByServiceId(string id);
        Task<List<BookingService>?> GetListRequestBookingByServiceId(string id);
        Task<bool?> Create(BookingService bookingService);
        Task UpdateStatus(BookingService bookingService);
        Task<bool?> UpdateStatus(string? bookingId, string? status);
        Task<List<BookingService>?> GetBookingsByExpert(string? expertId, string? status);
    }
}
