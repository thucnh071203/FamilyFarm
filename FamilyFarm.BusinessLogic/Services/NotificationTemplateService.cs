using FamilyFarm.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly Dictionary<string, string> _templates;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTemplateService"/> class
        /// with a predefined set of notification templates.
        /// </summary>
        public NotificationTemplateService()
        {
            _templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Chat", "You have a new message from {0}" },
                { "Post", "You have a new comment on your post from {0}" },
                { "FriendRequest", "{0} sent you a friend request" },
                { "Booking", "Your booking for {0} has been confirmed" },
                { "JoinedGroup", "{0} has joined your group" },
                { "Reaction", "{0} reacted to your post" },
                { "SharePost", "{0} shared your post" },
                { "Review", "{0} reviewed your service" },
                { "Report", "{0} reported your {1}" },
                { "SavedPost", "{0} saved your post" }
                // Thêm các template khác nếu cần
            };
        }

        /// <summary>
        /// Retrieves the notification message template associated with the specified category name.
        /// </summary>
        /// <param name="categoryName">The name of the notification category (e.g., "Chat", "Post").</param>
        /// <returns>
        /// A formatted notification template string if the category exists;
        /// otherwise, a default template: "You have a new notification from {0}".
        /// Returns null if the input is null or empty.
        /// </returns>
        public string GetNotificationTemplate(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return null;

            if (_templates.TryGetValue(categoryName, out var template))
            {
                return template;
            }

            return "You have a new notification from {0}";
        }
    }
}
