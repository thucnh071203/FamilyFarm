using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
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

        public NotificationService(
            INotificationRepository notificationRepository,
            INotificationStatusRepository notificationStatusRepository,
            IAccountRepository accountRepository,
            IHubContext<NotificationHub> notificationHubContext,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _notificationStatusRepository = notificationStatusRepository;
            _accountRepository = accountRepository;
            _notificationHubContext = notificationHubContext;
            _mapper = mapper;
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
                    Message = "No notification found!",
                    UnreadCount = 0,
                    Notifications = new List<Notification>()
                };
            }

            // Get all NotificationStatus entries for the user
            var statuses = await _notificationStatusRepository.GetByReceiverIdAsync(accId);
            if (!statuses.Any())
            {
                return new ListNotifiResponseDTO
                {
                    Success = true,
                    Message = "No notifications found for user.",
                    UnreadCount = 0,
                    Notifications = new List<Notification>()
                };
            }

            // Get Notification IDs
            var notifiIds = statuses.Select(s => s.NotifiId).ToList();

            // Get the corresponding Notifications using repository
            var notifications = await _notificationRepository.GetByNotifiIdsAsync(notifiIds);

            // Calculate unread count
            int unreadCount = statuses.Count(s => s.IsRead != true);

            return new ListNotifiResponseDTO
            {
                Success = true,
                Message = "Get list of notifications successfully!",
                UnreadCount = unreadCount,
                Notifications = notifications
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