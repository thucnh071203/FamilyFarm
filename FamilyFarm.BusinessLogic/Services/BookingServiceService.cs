using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Servers;
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
        //private readonly IHubContext<BookingHub> _bookingHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHubContext<AllHub> _allHub;
        private readonly INotificationService _notificationService;
        private readonly IStatisticService _statisticService;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        public BookingServiceService(IBookingServiceRepository repository, IAccountRepository accountRepository, IServiceRepository serviceRepository, IHubContext<NotificationHub> notificationHub, IPaymentRepository paymentRepository, IHubContext<AllHub> allHub, INotificationService notificationService, IStatisticService statisticService, IHubContext<TopEngagedPostHub> hubContext)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _serviceRepository = serviceRepository;
            //_bookingHub = bookingHub;
            _notificationHub = notificationHub;
            _paymentRepository = paymentRepository;
            _allHub = allHub;
            _notificationService = notificationService;
            _statisticService = statisticService;
            _hubContext = hubContext;
        }

        public async Task<bool?> CancelBookingService(string bookingServiceId)
        {
            if (string.IsNullOrEmpty(bookingServiceId)) return null;
            var bookingservice = await _repository.GetById(bookingServiceId);
            if (bookingservice == null) return null;
            bookingservice.BookingServiceStatus = "Cancel";
            bookingservice.CancelServiceAt = DateTime.Now;
            //try
            //{
            //    await _repository.UpdateStatus(bookingservice);
            //    await _bookingHub.Clients.All.SendAsync("ReceiveBookingStatusChanged", bookingServiceId, "Cancel");
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}
            try
            {
                await _repository.UpdateStatus(bookingservice);

                //await _hubContext.Clients.All.SendAsync("BookingCancelled", bookingservice);
                //await _hubContext.Clients.Group(bookingservice.ExpertId).SendAsync("BookingCancelled", bookingservice);
                await _hubContext.Clients
   .Group(bookingservice.ExpertId)
 .SendAsync("BookingCancelled", bookingservice);


                // 🔽 THÊM ĐOẠN NÀY SAU KHI UPDATE THÀNH CÔNG:
                var accId = bookingservice.AccId; // Hoặc bookingservice.Booking.AccId nếu dữ liệu dạng liên kết

                await _notificationHub.Clients.Group(accId).SendAsync(
                    "ReceiveBookingStatusChanged",
                    bookingServiceId,
                    "Cancel"
                );

                Console.WriteLine($"Sending to group {accId}: Booking {bookingServiceId} cancelled.");

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
            bookingservice.BookingServiceStatus = "Rejected";
            bookingservice.RejectServiceAt = DateTime.Now;
            try
            {
                await _repository.UpdateStatus(bookingservice);
                //await _hubContext.Clients.All.SendAsync("BookingRejected", bookingservice);
                await _hubContext.Clients.Group(bookingservice.ExpertId).SendAsync("BookingRejected", bookingservice);
                await _hubContext.Clients
.Group(bookingservice.ExpertId)
.SendAsync("BookingRejected", bookingservice);


                // Gửi SignalR đến Farmer
                var accId = bookingservice.AccId;
                if (!string.IsNullOrEmpty(accId))
                {
                    await _notificationHub.Clients.Group(accId).SendAsync("ReceiveBookingStatusChanged", bookingServiceId, "Rejected");
                }

                var service = await _serviceRepository.GetServiceById(bookingservice.ServiceId);

                var expert = await _accountRepository.GetAccountByIdAsync(bookingservice.ExpertId);

                var notiRequest = new SendNotificationRequestDTO
                {
                    ReceiverIds = new List<string> { bookingservice.AccId },
                    SenderId = expert.AccId,
                    CategoryNotiId = "685d3f6d1d2b7e9f45ae1c40",
                    TargetId = bookingServiceId,
                    TargetType = "Booking", //để link tới notifi gốc Post, Chat, Process, ...
                    Content = "Rejected booking of " + service?.ServiceName + "."
                };

                var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
                if (!notiResponse.Success)
                {
                    Console.WriteLine($"Notification failed: {notiResponse.Message}");
                }

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
                //await _hubContext.Clients.All.SendAsync("BookingAccepted", bookingservice);
                //await _hubContext.Clients.Group(bookingservice.ExpertId).SendAsync("BookingAccepted", bookingservice);
                await _hubContext.Clients
                .Group(bookingservice.ExpertId)
                .SendAsync("BookingAccepted", bookingservice);

                // Gửi SignalR đến Farmer
                var accId = bookingservice.AccId;
                if (!string.IsNullOrEmpty(accId))
                {
                    await _notificationHub.Clients.Group(accId).SendAsync("ReceiveBookingStatusChanged", bookingServiceId, "Accepted");
                }

                var service = await _serviceRepository.GetServiceById(bookingservice.ServiceId);

                var expert = await _accountRepository.GetAccountByIdAsync(bookingservice.ExpertId);

                var notiRequest = new SendNotificationRequestDTO
                {
                    ReceiverIds = new List<string> { bookingservice.AccId },
                    SenderId = expert.AccId,
                    CategoryNotiId = "685d3f6d1d2b7e9f45ae1c40",
                    TargetId = bookingServiceId,
                    TargetType = "Booking", //để link tới notifi gốc Post, Chat, Process, ...
                    Content = "Accepted booking of " + service?.ServiceName + "."
                };

                var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
                if (!notiResponse.Success)
                {
                    Console.WriteLine($"Notification failed: {notiResponse.Message}");
                }

                // Gửi signalR cập nhật weekly growth chart
                var weeklyData = await _statisticService.GetWeeklyBookingGrowthAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveWeeklyGrowthUpdate", weeklyData);


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
                var service = await _serviceRepository.GetByIdOutDelete(item.ServiceId);
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
                if (item.IsPaidByFarmer == false)
                {
                    // Cập nhật trạng thái khi quá hạn thanh toán
                    if (item.PaymentDueDate < DateTime.Today)
                    {
                        item.BookingServiceStatus = "Rejected";
                        item.RejectServiceAt = DateTime.Now;

                        await _repository.UpdateStatus(item);
                    }
                }
                
                var service = await _serviceRepository.GetByIdOutDelete(item.ServiceId);
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

            if (listService == null) return new BookingServiceResponseDTO
            {
                Success = false,
                Message = "You dont have service!"
            };

            foreach (var item in listService)
            {
                var service = await _repository.GetListRequestBookingByServiceId(item.ServiceId);
                listBooking.AddRange(service);
            }

            listBooking = listBooking
            .OrderByDescending(x => x.BookingServiceAt)
            .ToList();

            if (listBooking == null) return new BookingServiceResponseDTO
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

        public async Task<bool?> RequestToBookingService(string? accId, string? serviceId, string? description)
        {
            if (accId == null || serviceId == null || description == null)
                return null;

            var service = await _serviceRepository.GetServiceById(serviceId);
            if(service == null) return null;

            var bookingService = new BookingService();
            bookingService.AccId = accId;
            bookingService.ExpertId = service.ProviderId;
            bookingService.ServiceId = serviceId;
            bookingService.ServiceName = service.ServiceName;
            bookingService.Price = service.Price;
            bookingService.Description = description;
            bookingService.CommissionRate = service.Price * 0.10m; //Tính phần trăm hoa hồng 10%
            bookingService.BookingServiceAt = DateTime.Now;
            bookingService.PaymentDueDate = DateTime.Now.AddDays(3);
            bookingService.BookingServiceStatus = "Pending";
            bookingService.IsDeleted = false;
            bookingService.IsCompletedFinal = false;
            bookingService.IsPaidByFarmer = false;
            bookingService.IsPaidToExpert = false;
            bookingService.HasExtraProcess = false;

            var isRequestSuccess = await _repository.Create(bookingService);
            //await _hubContext.Clients.All.SendAsync("BookingCreated", bookingService);

            //await _hubContext.Clients.Group(bookingService.ExpertId).SendAsync("BookingCreated", bookingService);
            await _hubContext.Clients
    .Group(bookingService.ExpertId)
  .SendAsync("BookingCreated", bookingService);


            var expertId = service.ProviderId; // lấy từ Service tương ứng

            var lastTestBookingByFarmer = await _repository.GetLastestBookingByFarmer(accId);

            var farmer = await _accountRepository.GetAccountByAccId(accId);

            await _notificationHub.Clients.Group(expertId).SendAsync("ReceiveNewBookingRequest", new
            {
                BookingServiceId = lastTestBookingByFarmer.BookingServiceId,
                ServiceName = service.ServiceName,
                FarmerName = farmer.FullName, // nếu có
                FarmerAvatar = farmer.Avatar, // ✅ bổ sung avatar
                BookingServiceAt = lastTestBookingByFarmer.BookingServiceAt?.ToString("o"),
                Status = lastTestBookingByFarmer.BookingServiceStatus
            });

            var account = await _accountRepository.GetAccountByIdAsync(accId);

            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { service.ProviderId },
                SenderId = accId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c40",
                TargetId = lastTestBookingByFarmer.BookingServiceId,
                TargetType = "Booking", //để link tới notifi gốc Post, Chat, Process, ...
                Content = "Send you a request booking " + service?.ServiceName + "."
            };

            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);//send noti
            if (!notiResponse.Success)
            {
                Console.WriteLine($"Notification failed: {notiResponse.Message}");
            }

            return isRequestSuccess;
        }

        public async Task<bool?> UpdateStatusBooking(string? bookingId, string? status)
        {
            return await _repository.UpdateStatus(bookingId, status);
        }

        public async Task<BookingServiceResponseDTO?> GetListBookingPaid(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var listBooking = await _repository.GetBookingsByExpert(expertId, "Paid");
            if(listBooking == null)
                return new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "List paid booking of expert is invalid."
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

        public async Task<BookingServiceResponseDTO?> GetListBookingUnpaid(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var listBooking = await _repository.GetBookingsByExpert(expertId, "Accepted");
            if (listBooking == null)
                return new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "List paid booking of expert is invalid."
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

        public async Task<BookingServiceResponseDTO?> GetListExtraRequest(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var listBooking = await _repository.GetListExtraRequest(expertId);
            if (listBooking == null)
                return new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "List extra request booking of expert is invalid."
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

        public async Task<BookingServiceResponseDTO?> GetListBookingCompleted()
        {
            //var listBooking = (await _repository.GetAllBookingCompleted())
            //        .OrderByDescending(x => x.CompleteServiceAt)
            //        .ToList();

            var listBooking = await _repository.GetAllBookingCompleted();

            if (listBooking == null)
                return new BookingServiceResponseDTO
                {
                    Success = false,
                    Message = "List completed booking is invalid."
                };

            List<BookingServiceMapper> listResponse = new List<BookingServiceMapper>();

            foreach (var item in listBooking)
            {
                var service = await _serviceRepository.GetServiceById(item.ServiceId);
                var farmer = await _accountRepository.GetAccountById(item.AccId);
                var expert = await _accountRepository.GetAccountById(service.ProviderId);
                var payment = await _paymentRepository.GetRepaymentByBookingId(item.BookingServiceId);
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
                        WorkAt = farmer.WorkAt,
                        StudyAt = farmer.StudyAt,
                        Status = farmer.Status,
                    },
                    Expert = new ExpertMapper
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
                        WorkAt = expert.WorkAt,
                        StudyAt = expert.StudyAt,
                        Status = expert.Status,
                    },
                    Service = service,
                    Booking = item,
                    Payment = payment
                };
                listResponse.Add(mapper);

            }
            return new BookingServiceResponseDTO
            {
                Success = true,
                Data = listResponse,
            };
        }

        public async Task<bool?> RequestExtraProcessByBooking(CreateExtraProcessRequestDTO request)
        {
            if (request == null)
                return null;

            if (request.BookingId == null || request.ExtraDescription == null)
                return null;

            var currentBooking = await _repository.GetById(request.BookingId);

            if (currentBooking == null)
                return null;

            //CẬP NHẬT STATUS AND EXTRA DESCRIPTION
            currentBooking.BookingServiceStatus = "Extra Request";
            currentBooking.ExtraDescription = request.ExtraDescription;
            currentBooking.HasExtraProcess = true;

            var updatedBooking = await _repository.UpdateBooking(request.BookingId, currentBooking);

            if (updatedBooking == null)
                return false;

            return true;
        }
    }
}
