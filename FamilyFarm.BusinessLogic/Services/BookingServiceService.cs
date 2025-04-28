using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class BookingServiceService:IBookingServiceService
    {
        private readonly IBookingServiceRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly IServiceRepository _serviceRepository;
        public BookingServiceService(IBookingServiceRepository repository, IAccountRepository accountRepository, IServiceRepository serviceRepository)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<List<BookingService>?> CancelBookingService(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingServiceResponseDTO>?> ExpertResponseBookingService(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingServiceResponseDTO>?> GetAllBookingOfExpert(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingServiceResponseDTO>?> GetAllBookingOfFarmer(string id)
        {
            throw new NotImplementedException();
        }

        public Task<BookingService?> GetById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingServiceResponseDTO>?> GetRequestBookingOfExpert(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BookingServiceResponseDTO>?> GetRequestBookingOfFarmer(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool?> SendRequestBooking(string username, string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(username)) return null;
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            var farmer = await _accountRepository.GetAccountByUsername(username);

            if(farmer == null || service==null) return null;
            var bookingService = new BookingService { 
                BookingServiceId="",
                AccId = farmer.AccId,
                ServiceId= service.ServiceId,
                Price = service.Price,
                BookingServiceAt = DateTime.Now,
                BookingServiceStatus= "Pending",
                CancelServiceAt = null,
                RejectServiceAt = null,
                FirstPayment=null,
                FirstPaymentAt=null,
                SecondPayment=null, 
                SecondPaymentAt=null,
                IsDeleted=false,

            };
             await _repository.Create(bookingService);
            return true;
        }
    }
}
