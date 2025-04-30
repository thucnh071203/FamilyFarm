using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
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
    public class BookingServiceService : IBookingServiceService
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

        public async Task<bool?> CancelBookingService(string bookingServiceId)
        {
            if (string.IsNullOrEmpty(bookingServiceId)) return null;
            var bookingservice = await _repository.GetById(bookingServiceId);
            if (bookingservice == null) return null;
            bookingservice.BookingServiceStatus = "Cancel";
            bookingservice.CancelServiceAt = DateTime.Now;
            try
            {
                await _repository.UpdateStatus(bookingservice);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool?> ExpertRejectBookingService(string bookingServiceId)
        {
            if (string.IsNullOrEmpty(bookingServiceId)) return null;
            var bookingservice = await _repository.GetById(bookingServiceId);
            if (bookingservice == null) return null;
            bookingservice.BookingServiceStatus = "Reject";
            bookingservice.RejectServiceAt = DateTime.Now;
            try
            {
                await _repository.UpdateStatus(bookingservice);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }



        }
        public async Task<bool?> ExpertAcceptBookingService(string bookingServiceId)
        {

            if (string.IsNullOrEmpty(bookingServiceId)) return null;
            var bookingservice = await _repository.GetById(bookingServiceId);
            if (bookingservice == null) return null;
            bookingservice.BookingServiceStatus = "Accepted";
            try
            {
                await _repository.UpdateStatus(bookingservice);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public async Task<BookingServiceResponseDTO?> GetAllBookingOfExpert(string expertId)
        {
            if (string.IsNullOrEmpty(expertId)) return null;

            var listService = await _serviceRepository.GetAllServiceByProvider(expertId);

            var listBooking = new List<BookingService>();

            if (listService.Count == 0 || listService == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "You dont have service!"
            };

            foreach (var item in listService)
            {
                var service = await _repository.GetAllBookingByServiceId(item.ServiceId);
                listBooking.AddRange(service);
            }
            if (listBooking.Count == 0 || listBooking == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "Your services dont have booking!"
            };

            List<BookingServiceMapper> listResponse = new List<BookingServiceMapper>();
            foreach (var item in listBooking)
            {
                var service = await _serviceRepository.GetServiceById(item.ServiceId);
                var farmer = await _accountRepository.GetAccountById(item.AccId);
                var mapper = new BookingServiceMapper
                {
                    Account = new FriendMapper
                    {
                        AccId = farmer.AccId,
                        RoleId = farmer.RoleId,
                        Username = farmer.Username,
                        FullName = farmer.FullName,
                        Birthday = farmer.Birthday,
                        Gender = farmer.Gender,
                        City = farmer.City,
                        Country = farmer.Country,
                        Address = farmer.Address,
                        Avatar = farmer.Avatar,
                        Background = farmer.Background,
                        Certificate = farmer.Certificate,
                        WorkAt = farmer.WorkAt,
                        StudyAt = farmer.StudyAt,
                        Status = farmer.Status,

                    },
                    Service = service,
                    Booking = item,

                };
                listResponse.Add(mapper);

            }
            return new BookingServiceResponseDTO
            {
                Success = true,
                Data = listResponse,
            };
        }

        public async Task<BookingServiceResponseDTO?> GetAllBookingOfFarmer(string farmerId)
        {
            if (string.IsNullOrEmpty(farmerId)) return null;
            var listBooking = await _repository.GetAllBookingByAccid(farmerId);
            if (listBooking.Count == 0 || listBooking == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "You dont have booking service!"
            };
            List<BookingServiceMapper> listResponse = new List<BookingServiceMapper>();
            foreach (var item in listBooking)
            {
                var service = await _serviceRepository.GetServiceById(item.ServiceId);
                var expert = await _accountRepository.GetAccountById(service.ProviderId);
                var mapper = new BookingServiceMapper
                {
                    Account = new FriendMapper
                    {
                        AccId = expert.AccId,
                        RoleId = expert.RoleId,
                        Username = expert.Username,
                        FullName = expert.FullName,
                        Birthday = expert.Birthday,
                        Gender = expert.Gender,
                        City = expert.City,
                        Country = expert.Country,
                        Address = expert.Address,
                        Avatar = expert.Avatar,
                        Background = expert.Background,
                        Certificate = expert.Certificate,
                        WorkAt = expert.WorkAt,
                        StudyAt = expert.StudyAt,
                        Status = expert.Status,

                    },
                    Service = service,
                    Booking = item,

                };
                listResponse.Add(mapper);

            }
            return new BookingServiceResponseDTO
            {
                Success = true,
                Data = listResponse,
            };
        }

        public async Task<BookingService?> GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await _repository.GetById(id);
        }

        public async Task<BookingServiceResponseDTO?> GetRequestBookingOfExpert(string expertId)
        {
            if (string.IsNullOrEmpty(expertId)) return null;

            var listService = await _serviceRepository.GetAllServiceByProvider(expertId);

            var listBooking = new List<BookingService>();

            if (listService.Count == 0 || listService == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "You dont have service!"
            };

            foreach (var item in listService)
            {
                var service = await _repository.GetListRequestBookingByServiceId(item.ServiceId);
                listBooking.AddRange(service);
            }
            if (listBooking.Count == 0 || listBooking == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "Your services dont have booking!"
            };

            List<BookingServiceMapper> listResponse = new List<BookingServiceMapper>();
            foreach (var item in listBooking)
            {
                var service = await _serviceRepository.GetServiceById(item.ServiceId);
                var farmer = await _accountRepository.GetAccountById(item.AccId);
                var mapper = new BookingServiceMapper
                {
                    Account = new FriendMapper
                    {
                        AccId = farmer.AccId,
                        RoleId = farmer.RoleId,
                        Username = farmer.Username,
                        FullName = farmer.FullName,
                        Birthday = farmer.Birthday,
                        Gender = farmer.Gender,
                        City = farmer.City,
                        Country = farmer.Country,
                        Address = farmer.Address,
                        Avatar = farmer.Avatar,
                        Background = farmer.Background,
                        Certificate = farmer.Certificate,
                        WorkAt = farmer.WorkAt,
                        StudyAt = farmer.StudyAt,
                        Status = farmer.Status,

                    },
                    Service = service,
                    Booking = item,

                };
                listResponse.Add(mapper);

            }
            return new BookingServiceResponseDTO
            {
                Success = true,
                Data = listResponse,
            };
        }

        public async Task<BookingServiceResponseDTO?> GetRequestBookingOfFarmer(string farmerId)
        {
            if (string.IsNullOrEmpty(farmerId)) return null;

            var listBooking = await _repository.GetListRequestBookingByAccid(farmerId);
            if (listBooking == null || listBooking.Count == 0) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "You dont have request to book service!"
            };
            List<BookingServiceMapper> listResponse = new List<BookingServiceMapper>();
            foreach (var item in listBooking)
            {
                var service = await _serviceRepository.GetServiceById(item.ServiceId);
                var expert = await _accountRepository.GetAccountById(service.ProviderId);
                var mapper = new BookingServiceMapper
                {
                    Account = new FriendMapper
                    {
                        AccId = expert.AccId,
                        RoleId = expert.RoleId,
                        Username = expert.Username,
                        FullName = expert.FullName,
                        Birthday = expert.Birthday,
                        Gender = expert.Gender,
                        City = expert.City,
                        Country = expert.Country,
                        Address = expert.Address,
                        Avatar = expert.Avatar,
                        Background = expert.Background,
                        Certificate = expert.Certificate,
                        WorkAt = expert.WorkAt,
                        StudyAt = expert.StudyAt,
                        Status = expert.Status,

                    },
                    Service = service,
                    Booking = item,

                };
                listResponse.Add(mapper);

            }
            return new BookingServiceResponseDTO
            {
                Success = true,
                Data = listResponse,
            };


        }

        public async Task<bool?> SendRequestBooking(string username, string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(username)) return null;
            var service = await _serviceRepository.GetServiceById(serviceId);
            var farmer = await _accountRepository.GetAccountByUsername(username);

            if (farmer == null || service == null) return null;
            var bookingService = new BookingService
            {
                BookingServiceId = "",
                AccId = farmer.AccId,
                ServiceId = service.ServiceId,
                Price = service.Price,
                BookingServiceAt = DateTime.Now,
                BookingServiceStatus = "Pending",
                CancelServiceAt = null,
                RejectServiceAt = null,
                FirstPayment = null,
                FirstPaymentAt = null,
                SecondPayment = null,
                SecondPaymentAt = null,
                IsDeleted = false,

            };
            await _repository.Create(bookingService);
            return true;
        }
    }
}
