using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationStatusRepository _notificationStatusRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPostRepository _postRepository;
        private readonly IChatDetailRepository _chatDetailRepository;
        private readonly ICategoryNotificationRepository _categoryNotificationRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            INotificationStatusRepository notificationStatusRepository,
            IAccountRepository accountRepository,
            IHubContext<NotificationHub> notificationHubContext,
            IMapper mapper,
            IServiceRepository serviceRepository,
            IPostRepository postRepository,
            IChatDetailRepository chatDetailRepository,
            ICategoryNotificationRepository categoryNotificationRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationStatusRepository = notificationStatusRepository;
            _accountRepository = accountRepository;
            _notificationHubContext = notificationHubContext;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _postRepository = postRepository;
            _chatDetailRepository = chatDetailRepository;
            _categoryNotificationRepository = categoryNotificationRepository;
        }

        public async Task<SendNotificationResponseDTO> SendNotificationAsync(SendNotificationRequestDTO request)
        {
            // Validate ReceiverIds
            foreach (var receiverId in request.ReceiverIds)
            {
                if (!ObjectId.TryParse(receiverId, out _))
                {
                    return new SendNotificationResponseDTO
                    {
                        Success = false,
                        Message = $"Invalid receiverId: {receiverId}"
                    };
                }
            }

            // Map the request DTO to the Notification entity
            var notification = _mapper.Map<Notification>(request);

            // If SenderId is not a valid ObjectId, set it to null
            if (!ObjectId.TryParse(request.SenderId, out _))
                notification.SenderId = null;

            // Save the notification to the database
            var savedNotification = await _notificationRepository.CreateAsync(notification);
            if (savedNotification == null)
            {
                return new SendNotificationResponseDTO
                {
                    Success = false,
                    Message = "Failed to create notification."
                };
            }

            // Create NotificationStatus for each receiver
            var statuses = request.ReceiverIds.Select(receiverId => new NotificationStatus
            {
                NotifiStatusId = ObjectId.GenerateNewId().ToString(),
                NotifiId = savedNotification.NotifiId,
                AccId = receiverId,
                IsRead = false
            }).ToList();

            await _notificationStatusRepository.CreateManyAsync(statuses);

            // Send the notification in real-time using SignalR to each receiver
            foreach (var receiverId in request.ReceiverIds)
            {
                var status = statuses.FirstOrDefault(s => s.AccId == receiverId);
                var notificationDTO = _mapper.Map<NotificationDTO>(savedNotification);
                notificationDTO.Status = new NotificationStatus
                {
                    NotifiStatusId = status.NotifiStatusId,
                    NotifiId = status.NotifiId,
                    AccId = status.AccId,
                    IsRead = status.IsRead
                };
                // Điền thêm thông tin cho NotificationDTO
                var sender = await _accountRepository.GetAccountByIdAsync(savedNotification.SenderId);
                var category = await _categoryNotificationRepository.GetByIdAsync(savedNotification.CategoryNotifiId);
                notificationDTO.SenderName = sender?.FullName;
                notificationDTO.SenderAvatar = sender?.Avatar;
                notificationDTO.CategoryName = category?.CategoryNotifiName;

                Console.WriteLine($"Sending notification to {receiverId}: {Newtonsoft.Json.JsonConvert.SerializeObject(notificationDTO)}");
                await _notificationHubContext.Clients.Group(receiverId).SendAsync("ReceiveNotification", notificationDTO);
            }

            return new SendNotificationResponseDTO
            {
                Success = true,
                Message = "Notification sent successfully.",
                Data = savedNotification
            };
        }

        public async Task<ListNotifiResponseDTO> GetNotificationsForUserAsync(string accId)
        {
            if (!ObjectId.TryParse(accId, out _))
            {
                return new ListNotifiResponseDTO
                {
                    Success = false,
                    Message = "Invalid account ID!",
                    UnreadCount = 0,
                    Notifications = new List<NotificationDTO>()
                };
            }

            // Lấy NotificationStatus của người dùng
            var statuses = await _notificationStatusRepository.GetByReceiverIdAsync(accId);
            if (!statuses.Any())
            {
                return new ListNotifiResponseDTO
                {
                    Success = true,
                    Message = "No notifications found for user.",
                    UnreadCount = 0,
                    Notifications = new List<NotificationDTO>()
                };
            }

            var notifiIds = statuses.Select(s => s.NotifiId).ToList();
            var notifications = await _notificationRepository.GetByNotifiIdsAsync(notifiIds);

            var notificationDTOs = new List<NotificationDTO>();

            foreach (var notification in notifications)
            {
                Account? sender = null;

                sender = await _accountRepository.GetAccountByIdAsync(notification.SenderId);
                if (sender == null)
                {
                    Console.WriteLine($"No sender found for notification {notification.NotifiId}");
                }

                // Lấy target title dựa vào type
                string? targetContent = null;
                switch (notification.TargetType?.ToLower())
                {
                    case "post":
                        var post = await _postRepository.GetPostById(notification.TargetId);
                        targetContent = post?.PostContent;
                        break;
                    case "service":
                        var service = await _serviceRepository.GetServiceById(notification.TargetId);
                        targetContent = service?.ServiceName;
                        break;
                    case "chat":
                        var chat = await _chatDetailRepository.GetChatDetailsByAccIdsAsync(accId, notification.TargetId);
                        targetContent = chat.LastOrDefault()?.Message;
                        break;
                }

                var category = await _categoryNotificationRepository.GetByIdAsync(notification.CategoryNotifiId);
                var notifiStatus = await _notificationStatusRepository.GetByAccAndNotifiAsync(accId, notification.NotifiId);

                if (category == null)
                {
                    return new ListNotifiResponseDTO
                    {
                        Success = false,
                        Message = "Get list notifications failed!"
                    };
                }

                var notificationDTO = new NotificationDTO
                {
                    NotifiId = notification.NotifiId,
                    Content = notification.Content,
                    CreatedAt = notification.CreatedAt,

                    SenderId = notification.SenderId,
                    SenderName = sender?.FullName,
                    SenderAvatar = sender?.Avatar,

                    CategoryNotifiId = category.CategoryNotifiId,
                    CategoryName = category.CategoryNotifiName,

                    TargetId = notification.TargetId,
                    TargetType = notification.TargetType,
                    TargetContent = targetContent,

                    Status = notifiStatus
                };

                notificationDTOs.Add(notificationDTO);
            }

            return new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = statuses.Count(s => !s.IsRead),
                Notifications = notificationDTOs
            };
        }

        public async Task<bool> MarkAsReadByNotificationIdAsync(string notifiStatusId)
        {
            if (!ObjectId.TryParse(notifiStatusId, out _))
                return false;

            var notification = await _notificationStatusRepository.GetByIdAsync(notifiStatusId);
            if (notification == null)
                return false;

            return await _notificationStatusRepository.MarkAllAsReadByNotifiIdAsync(notifiStatusId);
        }

        public async Task<bool> MarkAllAsReadByAccIdAsync(string accId)
        {
            if (!ObjectId.TryParse(accId, out _))
                return false;

            var account = await _accountRepository.GetAccountByIdAsync(accId);
            if (account == null)
                return false;

            return await _notificationStatusRepository.MarkAllAsReadByAccIdAsync(accId);
        }
    }
}