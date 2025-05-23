﻿using FamilyFarm.Models.DTOs.Response;
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
        Task<BookingServiceResponseDTO?> GetAllBookingOfExpert(string expertId);
        Task<BookingServiceResponseDTO?> GetAllBookingOfFarmer(string farmerId);
        Task<BookingServiceResponseDTO?> GetRequestBookingOfExpert(string expertId);
        Task<BookingServiceResponseDTO?> GetRequestBookingOfFarmer(string farmerId);
        Task<bool?> ExpertAcceptBookingService (string bookingServiceId);
        Task<bool?> ExpertRejectBookingService(string bookingServiceId);
        Task<bool?> CancelBookingService(string bookingServiceId);
        Task<bool?> SendRequestBooking(string username, string serviceId);
    }
}
