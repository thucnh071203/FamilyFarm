using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class StatisticDAO
    {
        private readonly IMongoCollection<Post> Posts;

        private readonly IMongoCollection<Role> _Role;
        private readonly IMongoCollection<PostCategory> PostCategorys;
        private readonly IMongoCollection<Account> _Account;
        private readonly PostDAO _postDAO;
        private readonly IMongoCollection<BookingService> BookingServices;
            private readonly IMongoCollection<PaymentTransaction> PaymentTransaction;
        private readonly IMongoCollection<Comment> Comments;
        private readonly IMongoCollection<Service> Services;
        private readonly IMongoCollection<CategoryService> CategoryServices;



        public StatisticDAO(IMongoDatabase database, PostDAO postDAO)
        {
            Posts = database.GetCollection<Post>("Post");
            PostCategorys = database.GetCollection<PostCategory>("PostCategory");
            _Account = database.GetCollection<Account>("Account");
            _postDAO = postDAO;
            BookingServices = database.GetCollection<BookingService>("BookingService");
            PaymentTransaction = database.GetCollection<PaymentTransaction>("Payment");
            Comments = database.GetCollection<Comment>("Comment");
            Services = database.GetCollection<Service>("Service");
            CategoryServices = database.GetCollection<CategoryService>("CategoryService");
            _Role = database.GetCollection<Role>("Role");
        }


        public async Task<long> CountPostsAsync()
        {
            var filter = Builders<Post>.Filter.Eq(p => p.IsDeleted, false);
            return await Posts.CountDocumentsAsync(filter);
        }

        public async Task<ExpertRevenueDTO> GetExpertRevenueAsync(string expertId, DateTime? from = null, DateTime? to = null)
        {
            // B1: Lấy danh sách booking services của expert
            var bookingFilter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.Eq(bs => bs.ExpertId, expertId),
                Builders<BookingService>.Filter.Eq(bs => bs.IsPaidByFarmer, true),
                Builders<BookingService>.Filter.Ne(bs => bs.IsDeleted, true)
            );

            var bookingServices = await BookingServices.Find(bookingFilter).ToListAsync();

            if (!bookingServices.Any())
                return new ExpertRevenueDTO(); // Không có booking → return rỗng

            // B2: Tạo dictionary cho nhanh
            var bookingServiceDict = bookingServices
                .Where(bs => bs.BookingServiceId != null)
                .ToDictionary(bs => bs.BookingServiceId, bs => bs);

            var bookingServiceIds = bookingServiceDict.Keys.ToList(); // List<ObjectId>

            // B3: Truy vấn payment transaction dựa theo BookingServiceId
            var paymentFilter = Builders<PaymentTransaction>.Filter.In(
                pt => pt.BookingServiceId, bookingServiceIds
            );

            var paymentTransactions = await PaymentTransaction.Find(paymentFilter).ToListAsync();

            // B4: Filter theo PayAt nếu có
            if (from.HasValue || to.HasValue)
            {
                paymentTransactions = paymentTransactions
                    .Where(pt => pt.PayAt.HasValue &&
                                 (!from.HasValue || pt.PayAt.Value >= from.Value) &&
                                 (!to.HasValue || pt.PayAt.Value <= to.Value))
                    .ToList();
            }

            // B5: Thống kê
            decimal totalRevenue = 0;
            decimal commissionRevenue = 0;
            int serviceCount = 0;
            var monthlyRevenue = new Dictionary<string, decimal>();
            var monthlyCommission = new Dictionary<string, decimal>();
            var serviceNameStats = new Dictionary<string, int>();
            var dailyRevenue = new Dictionary<string, decimal>();
            var dailyCommission = new Dictionary<string, decimal>();

            foreach (var payment in paymentTransactions)
            {
                if (!payment.PayAt.HasValue) continue;
                if (!bookingServiceDict.TryGetValue(payment.BookingServiceId, out var service)) continue;

                var price = service.Price ?? 0;
                var commissionRate = (service.CommissionRate ?? 0) / 100m;
                var commission = price * commissionRate;
                totalRevenue += price;
                commissionRevenue += commission;
                serviceCount++;

                // Theo tháng
                var monthKey = payment.PayAt.Value.ToString("yyyy-MM");
                if (!monthlyRevenue.ContainsKey(monthKey))
                {
                    monthlyRevenue[monthKey] = 0;
                    monthlyCommission[monthKey] = 0;
                }
                monthlyRevenue[monthKey] += price;
                monthlyCommission[monthKey] += commission;

                // Theo ngày
                var dayKey = payment.PayAt.Value.ToString("yyyy-MM-dd");
                if (!dailyRevenue.ContainsKey(dayKey))
                {
                    dailyRevenue[dayKey] = 0;
                    dailyCommission[dayKey] = 0;
                }
                dailyRevenue[dayKey] += price;
                dailyCommission[dayKey] += commission;

                // Thống kê top dịch vụ
                if (!string.IsNullOrEmpty(service.ServiceName))
                {
                    if (!serviceNameStats.ContainsKey(service.ServiceName))
                        serviceNameStats[service.ServiceName] = 0;
                    serviceNameStats[service.ServiceName]++;
                }
            }

            var topServiceNames = serviceNameStats
                .OrderByDescending(x => x.Value)
                .Take(3)
                .Select(x => x.Key)
                .ToList();

            return new ExpertRevenueDTO
            {
                ExpertId = expertId,
                TotalRevenue = totalRevenue,
                CommissionRevenue = commissionRevenue,
                TotalServicesProvided = serviceCount,
                MonthlyRevenue = monthlyRevenue,
                MonthlyCommission = monthlyCommission,
                TopServiceNames = topServiceNames,
                DailyRevenue = dailyRevenue,
                DailyCommission = dailyCommission
            };
        }
        public async Task<List<BookingService>> FindPaidBookingsAsync(DateTime? from = null, DateTime? to = null)
        {
            var fb = Builders<BookingService>.Filter;
            var filter = fb.Eq(bs => bs.IsPaidByFarmer, true);

            if (from.HasValue)
                filter &= fb.Gte(bs => bs.BookingServiceAt, from.Value);
            if (to.HasValue)
                filter &= fb.Lte(bs => bs.BookingServiceAt, to.Value);

            return await BookingServices.Find(filter).ToListAsync();
        }


        public async Task<List<EngagedPostResponseDTO>> GetTopEngagedPostsAsync(
   int topN, ReactionDAO reactionDAO, CommentDAO commentDAO)
        {
            var posts = await _postDAO.GetAllPostsAsync();

            var engagedPosts = new List<EngagedPostResponseDTO>();

            foreach (var post in posts)
            {
                var reactions = await reactionDAO.GetAllByEntityAsync(post.PostId, "Post");
                var comments = await commentDAO.GetAllByPostAsync(post.PostId);

                var dto = new EngagedPostResponseDTO
                {
                    Post = post,
                    TotalReactions = reactions.Count(),
                    TotalComments = comments.Count()
                };

                engagedPosts.Add(dto);
            }

            return engagedPosts
                .OrderByDescending(p => p.TotalEngagement)
                .Take(topN)
                .ToList();
        }



        public async Task<Dictionary<string, int>> GetWeeklyBookingGrowthAsync()
        {
            DateTime fromDate = DateTime.Now.AddMonths(-3);
            DateTime toDate = DateTime.Now;

            var filterBuilder = Builders<BookingService>.Filter;
            var filter = filterBuilder.Gte(x => x.BookingServiceAt, fromDate)
                        & filterBuilder.Lte(x => x.BookingServiceAt, toDate)
                        & (filterBuilder.Eq(x => x.IsDeleted, false) | filterBuilder.Eq(x => x.IsDeleted, null));

            var bookingList = await BookingServices.Find(filter).ToListAsync();

            var groupedByWeek = bookingList
                .GroupBy(x =>
                {
                    var date = x.BookingServiceAt ?? DateTime.MinValue;
                    var culture = System.Globalization.CultureInfo.CurrentCulture;
                    var calendar = culture.Calendar;
                    int week = calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    return new { date.Year, Week = week };
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Week,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Week)
                .ToDictionary(
                    x => $"Tuần {x.Week} - {x.Year}",
                    x => x.Count
                );

            return groupedByWeek;
        }




        public async Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate)
        {
            // Truy vấn bài viết
            var posts = await Posts
                .Find(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted)
                .ToListAsync();

            // Truy vấn bình luận
            var comments = await Comments
                .Find(c => c.CreateAt >= startDate && c.CreateAt <= endDate && (c.IsDeleted == null || c.IsDeleted == false))
                .ToListAsync();

            // Truy vấn dịch vụ đã đặt
            var bookings = await BookingServices
                .Find(b => b.BookingServiceAt >= startDate && b.BookingServiceAt <= endDate && b.IsDeleted != true)
                .ToListAsync();

            // Truy vấn thanh toán mới
            var payments = await PaymentTransaction
                .Find(p => p.PayAt >= startDate && p.PayAt <= endDate)
                .ToListAsync();

            var memberActivity = new List<MemberActivityResponseDTO>();

            // Nhóm bài viết theo tài khoản
            var postGroupedByAccount = posts
                .GroupBy(p => p.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Nhóm bình luận theo tài khoản
            var commentGroupedByAccount = comments
                .GroupBy(c => c.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Nhóm dịch vụ theo tài khoản
            var bookingGroupedByAccount = bookings
                .GroupBy(b => b.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Nhóm thanh toán theo tài khoản (tài khoản thanh toán)
            var paymentGroupedByAccount = payments
                .GroupBy(p => p.FromAccId)
                .ToDictionary(g => g.Key, g => g.Count());

            var accountIds = postGroupedByAccount.Keys
                .Concat(commentGroupedByAccount.Keys)
                .Concat(bookingGroupedByAccount.Keys)
                .Concat(paymentGroupedByAccount.Keys)
                .Distinct()
                .ToList();

            var accounts = await _Account
                .Find(a => accountIds.Contains(a.AccId))
                .ToListAsync();

            var roleIds = accounts
                .Where(a => !string.IsNullOrEmpty(a.RoleId))
                .Select(a => a.RoleId)
                .Distinct()
                .ToList();

            var roles = await _Role
                .Find(r => roleIds.Contains(r.RoleId))
                .ToListAsync();

            foreach (var accountId in accountIds)
            {
                var totalPosts = postGroupedByAccount.ContainsKey(accountId) ? postGroupedByAccount[accountId] : 0;
                var totalComments = commentGroupedByAccount.ContainsKey(accountId) ? commentGroupedByAccount[accountId] : 0;
                var totalBookings = bookingGroupedByAccount.ContainsKey(accountId) ? bookingGroupedByAccount[accountId] : 0;
                var totalPayments = paymentGroupedByAccount.ContainsKey(accountId) ? paymentGroupedByAccount[accountId] : 0;

                var accountInfo = accounts.FirstOrDefault(a => a.AccId == accountId);
                var roleInfo = roles.FirstOrDefault(r => r.RoleId == accountInfo?.RoleId);

                var memberDTO = new MemberActivityResponseDTO
                {
                    AccId = accountId,
                    AccountName = accountInfo?.FullName,
                    AccountAddress = accountInfo?.Address,
                    RoleName = roleInfo?.RoleName,
                    TotalPosts = totalPosts,
                    TotalComments = totalComments,
                    TotalBookings = totalBookings,
                    TotalPayments = totalPayments,
                    TotalActivity = totalPosts + totalComments + totalBookings + totalPayments
                };

                memberActivity.Add(memberDTO);
            }

            return memberActivity.OrderByDescending(m => m.TotalActivity).Take(10).ToList();
        }



        public async Task<List<UserByProvinceResponseDTO>> GetUsersByProvinceAsync()
        {
            var result = await _Account
                .Aggregate()
                .Group(a => a.City, g => new
                {
                    Province = g.Key,
                    UserCount = g.Count()
                })
                .SortByDescending(x => x.UserCount) 
                .ToListAsync(); 

            return result.Select(x => new UserByProvinceResponseDTO
            {
                Province = x.Province,
                UserCount = x.UserCount
            }).ToList();
        }


        //public async Task<Dictionary<string, List<BookingServiceByStatusDTO>>> CountByStatusAsync(string accId)
        //{
        //    var bookingFilter = Builders<BookingService>.Filter.Eq(x => x.AccId, accId);
        //    var bookings = await BookingServices.Find(bookingFilter).ToListAsync();

        //    var serviceIds = bookings.Select(b => b.ServiceId).Distinct().ToList();
        //    var serviceFilter = Builders<Service>.Filter.In(s => s.ServiceId, serviceIds);
        //    var services = await Services.Find(serviceFilter).ToListAsync();

        //    var result = bookings.Select(b =>
        //    {
        //        var service = services.FirstOrDefault(s => s.ServiceId == b.ServiceId);

        //        return new BookingServiceByStatusDTO
        //        {
        //            BookingServiceId = b.BookingServiceId,
        //            AccId = b.AccId,
        //            ServiceId = b.ServiceId,
        //            Price = b.Price,
        //            BookingServiceAt = b.BookingServiceAt,
        //            BookingServiceStatus = b.BookingServiceStatus,
        //            CancelServiceAt = b.CancelServiceAt,
        //            RejectServiceAt = b.RejectServiceAt,
        //            FirstPayment = b.FirstPayment,
        //            FirstPaymentAt = b.FirstPaymentAt,
        //            SecondPayment = b.SecondPayment,
        //            SecondPaymentAt = b.SecondPaymentAt,
        //            IsDeleted = b.IsDeleted,

        //            ServiceName = service?.ServiceName,
        //            ServiceDescription = service?.ServiceDescription
        //        };
        //    }).ToList();

        //    return result
        //        .GroupBy(x => x.BookingServiceStatus ?? "UNKNOWN")
        //        .ToDictionary(g => g.Key, g => g.ToList());
        //}

        //public async Task<Dictionary<string, List<BookingService>>> CountByStatusAsync(string accId)
        //{
        //    var filter = Builders<BookingService>.Filter.Eq(x => x.AccId, accId);
        //    var bookings = await BookingServices.Find(filter).ToListAsync();

        //    var grouped = bookings
        //        .GroupBy(x => x.BookingServiceStatus ?? "UNKNOWN")
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    return grouped;
        //}
        public async Task<List<BookingService>> GetBookingsByStatusAsync(string accId, string status)
        {
            var fb = Builders<BookingService>.Filter;
            var filter = fb.Eq(x => x.ExpertId, accId) &
                         fb.Eq(x => x.BookingServiceStatus, status) &
                         fb.Ne(x => x.IsDeleted, true);

            return await BookingServices.Find(filter).ToListAsync();
        }


        public async Task<Dictionary<string, int>> CountByDateAsync(string accId, string time)
        {
            var filter = Builders<BookingService>.Filter.Eq(x => x.AccId, accId);

            var data = await BookingServices
                .Find(filter)
                .ToListAsync();

            var grouped = data
                .Where(x => x.BookingServiceAt.HasValue)
                .GroupBy(x =>
                {
                    var date = x.BookingServiceAt.Value;
                    return time.ToLower() switch
                    {
                        "day" => date.ToString("yyyy-MM-dd"),
                        "month" => date.ToString("yyyy-MM"),
                        "year" => date.ToString("yyyy"),
                        _ => date.ToString("yyyy-MM-dd")
                    };
                })
                .ToDictionary(g => g.Key, g => g.Count());

            return grouped;
        }


        public async Task<Dictionary<string, int>> GetCountByMonthAsync(string accId, int year)
        {
            var filter = Builders<BookingService>.Filter.Where(x =>
                x.AccId == accId &&
                x.BookingServiceAt.HasValue &&
                x.BookingServiceAt.Value.Year == year &&
                (x.IsDeleted == null || x.IsDeleted == false));

            var bookings = await BookingServices.Find(filter).ToListAsync();

            return bookings
                .GroupBy(b => b.BookingServiceAt.Value.Month)
                .OrderBy(g => g.Key)
                .ToDictionary(g => $"Month {g.Key}", g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetCountByDayAllMonthsAsync(string accId, int year)
        {
            var filter = Builders<BookingService>.Filter.Where(x =>
                x.AccId == accId &&
                x.BookingServiceAt.HasValue &&
                x.BookingServiceAt.Value.Year == year &&
                (x.IsDeleted == null || x.IsDeleted == false));

            var bookings = await BookingServices.Find(filter).ToListAsync();

            return bookings
                .GroupBy(b => b.BookingServiceAt.Value.Date) 
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.ToString("dd/MM/yyyy"),
                    g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetPopularServiceCategoriesAsync(string accId)
        {
            // Lọc các category theo accId
            var categoryFilter = Builders<CategoryService>.Filter.Where(c =>
                c.AccId == accId &&
                (c.IsDeleted == null || c.IsDeleted == false));
            var categories = await CategoryServices.Find(categoryFilter).ToListAsync();

            var categoryDict = categories.ToDictionary(c => c.CategoryServiceId, c => c.CategoryName);

            // Lọc các service còn hoạt động
            var serviceFilter = Builders<Service>.Filter.Where(s =>
                categoryDict.Keys.Contains(s.CategoryServiceId) &&
                (s.IsDeleted == null || s.IsDeleted == false));
            var services = await Services.Find(serviceFilter).ToListAsync();

            return services
                .GroupBy(s => s.CategoryServiceId)
                .ToDictionary(
                    g => categoryDict.TryGetValue(g.Key, out var name) ? name : "Fail",
                    g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetMostBookedServicesByExpertAsync(string accId)
        {
            // 1. Lấy danh sách service của expert
            var expertServices = await Services
                .Find(s => s.ProviderId == accId)
                .ToListAsync();

            var serviceIdNameDict = expertServices.ToDictionary(s => s.ServiceId, s => s.ServiceName);
            var expertServiceIds = serviceIdNameDict.Keys.ToList(); // List<string>

            // 2. Lọc booking theo ServiceId dạng string
            var bookingFilter = Builders<BookingService>.Filter.And(
                Builders<BookingService>.Filter.In(b => b.ServiceId, expertServiceIds),
                Builders<BookingService>.Filter.Eq(b => b.BookingServiceStatus, "Accept")
            );

            var bookings = await BookingServices.Find(bookingFilter).ToListAsync();

            // 3. Nhóm theo ServiceId và đếm
            var result = bookings
                .GroupBy(b => b.ServiceId)
                .ToDictionary(
                    g => serviceIdNameDict.TryGetValue(g.Key, out var name) ? name : "Unknown",
                    g => g.Count()
                );

            return result;
        }


    }
}
