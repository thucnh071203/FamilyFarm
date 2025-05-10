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

        /// <summary>
        /// Sends a notification to a list of receivers, saves it to the database, 
        /// creates corresponding status records, and broadcasts it via SignalR.
        /// </summary>
        /// <param name="request">The request containing notification details and receiver IDs.</param>
        /// <returns>A response DTO indicating success, failure, and related messages or data.</returns>
        public async Task<SendNotificationResponseDTO> SendNotificationAsync(SendNotificationRequestDTO request)
        {
            // Validate input request data
            if (request == null
                || request.ReceiverIds == null || !request.ReceiverIds.Any()
                || string.IsNullOrEmpty(request.Content)
                || string.IsNullOrEmpty(request.CategoryNotiId)
                || !ObjectId.TryParse(request.CategoryNotiId, out _)
                || !ObjectId.TryParse(request.TargetId, out _))
            {
                return new SendNotificationResponseDTO
                {
                    Success = false,
                    Message = "Invalid notification input data."
                };
            }

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
                await _notificationHubContext.Clients.User(receiverId)
                    .SendAsync("ReceiveNotification", _mapper.Map<Notification>(savedNotification));
            }

            return new SendNotificationResponseDTO
            {
                Success = true,
                Message = "Notification sent successfully.",
                Data = savedNotification
            };
        }

        /// <summary>
        /// Retrieves all notifications for a specific user based on their account ID.
        /// </summary>
        /// <param name="accId">The ID of the account to retrieve notifications for.</param>
        /// <returns>A response DTO containing the list of notifications and unread count.</returns>
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

                if (!string.IsNullOrEmpty(notification.SenderId))
                {
                    sender = await _accountRepository.GetAccountByIdAsync(notification.SenderId);
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

                    // Add more later...
                }

                var category = await _categoryNotificationRepository.GetByIdAsync(notification.CategoryNotifiId);
                var notifiStatus = await _notificationStatusRepository.GetByAccAndNotifiAsync(accId, notification.NotifiId);

                if (category == null)
                {
                    // Xử lý trường hợp không tìm thấy category
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
                    
                    IsRead = notifiStatus.IsRead
                };

                notificationDTOs.Add(notificationDTO);
            }

            return new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = statuses.Count(s => s.IsRead != true),
                Notifications = notificationDTOs
            };
        }

        /// <summary>
        /// Marks a specific notification (by its status ID) as read.
        /// </summary>
        /// <param name="notifiStatusId">The ID of the notification status to mark as read.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public async Task<bool> MarkAsReadByNotificationIdAsync(string notifiStatusId)
        {
            if (!ObjectId.TryParse(notifiStatusId, out _))
                return false;

            var notification = await _notificationStatusRepository.GetByIdAsync(notifiStatusId);
            if (notification == null)
                return false;

            // Update status in NotificationStatus
            return await _notificationStatusRepository.MarkAllAsReadByNotifiIdAsync(notifiStatusId);
        }

        /// <summary>
        /// Marks all notifications for a specific account as read.
        /// </summary>
        /// <param name="accId">The account ID whose notifications should be marked as read.</param>
        /// <returns>True if successful; otherwise, false.</returns>
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