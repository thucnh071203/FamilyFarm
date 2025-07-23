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
        private readonly IMongoCollection<PaymentTransaction> PaymentTransactions;



        public StatisticDAO(IMongoDatabase database, PostDAO postDAO)
        {
            Posts = database.GetCollection<Post>("Post");
            PostCategorys = database.GetCollection<PostCategory>("PostCategory");
            _Account = database.GetCollection<Account>("Account");
            _postDAO = postDAO;
            BookingServices = database.GetCollection<BookingService>("BookingService");
            PaymentTransaction = database.GetCollection<PaymentTransaction>("PaymentTransaction");
            Comments = database.GetCollection<Comment>("Comment");
            Services = database.GetCollection<Service>("Service");
            CategoryServices = database.GetCollection<CategoryService>("CategoryService");
            PaymentTransactions = database.GetCollection<PaymentTransaction>("Payment");
            _Role = database.GetCollection<Role>("Role");
        }



        public async Task<ExpertRevenueDTO> GetExpertRevenueAsync(string expertId, DateTime? from = null, DateTime? to = null)
        {
            var filterBuilder = Builders<BookingService>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(bs => bs.ExpertId, expertId),
                filterBuilder.Eq(bs => bs.IsPaidByFarmer, true),
                filterBuilder.Ne(bs => bs.IsDeleted, false)
            );

            var services = await BookingServices.Find(filter).ToListAsync();

            // Lấy tất cả payment theo BookingServiceId
            var bookingIds = services.Select(bs => bs.BookingServiceId).ToList();
            var paymentFilter = Builders<PaymentTransaction>.Filter.In(p => p.BookingServiceId, bookingIds);
            var payments = await PaymentTransactions.Find(paymentFilter).ToListAsync();

            // Mapping BookingId -> PayAt
            var payAtDict = payments
                .Where(p => p.PayAt.HasValue)
                .ToDictionary(p => p.BookingServiceId!, p => p.PayAt!.Value);

            // Bắt đầu xử lý
            decimal totalRevenue = 0;
            decimal commissionRevenue = 0;
            int serviceCount = 0;
            var monthlyRevenue = new Dictionary<string, decimal>();
            var monthlyCommission = new Dictionary<string, decimal>();
            var dailyRevenue = new Dictionary<string, decimal>();
            var dailyCommission = new Dictionary<string, decimal>();
            var serviceNameStats = new Dictionary<string, int>();

            foreach (var service in services)
            {
                if (service.BookingServiceId == null || !payAtDict.ContainsKey(service.BookingServiceId)) continue;

                var payAt = payAtDict[service.BookingServiceId];

                // Kiểm tra khoảng thời gian
                if ((from.HasValue && payAt < from.Value) || (to.HasValue && payAt > to.Value)) continue;

                var price = service.Price ?? 0;
                var commissionRate = (service.CommissionRate ?? 0) / 100m;
                var commission = price * commissionRate;
                totalRevenue += price;
                commissionRevenue += commission;
                serviceCount++;

                var monthKey = payAt.ToString("yyyy-MM");
                if (!monthlyRevenue.ContainsKey(monthKey))
                {
                    monthlyRevenue[monthKey] = 0;
                    monthlyCommission[monthKey] = 0;
                }
                monthlyRevenue[monthKey] += price;
                monthlyCommission[monthKey] += commission;

                var dateKey = payAt.ToString("yyyy-MM-dd");
                if (!dailyRevenue.ContainsKey(dateKey))
                {
                    dailyRevenue[dateKey] = 0;
                    dailyCommission[dateKey] = 0;
                }
                dailyRevenue[dateKey] += price;
                dailyCommission[dateKey] += commission;

                if (!string.IsNullOrEmpty(service.ServiceName))
                {
                    if (!serviceNameStats.ContainsKey(service.ServiceName))
                        serviceNameStats[service.ServiceName] = 0;
                    serviceNameStats[service.ServiceName]++;
                }
            }

            var topServiceNames = serviceNameStats
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key)
                .ToList();

            return new ExpertRevenueDTO
            {
                ExpertId = expertId,
                TotalRevenue = totalRevenue,
                CommissionRevenue = commissionRevenue,
                TotalServicesProvided = serviceCount,
                MonthlyRevenue = monthlyRevenue,
                MonthlyCommission = monthlyCommission,
                DailyRevenue = dailyRevenue,
                DailyCommission = dailyCommission,
                TopServiceNames = topServiceNames
            };
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


        //    public async Task<List<MemberActivityResponseDTO>> GetMostActiveMembersAsync(DateTime startDate, DateTime endDate)
        //    {

        //        var posts = await Posts
        //  .Find(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted)
        //  .ToListAsync();

        //        var comments = await Comments
        //            .Find(c => c.CreateAt >= startDate && c.CreateAt <= endDate && (c.IsDeleted == null || c.IsDeleted == false))
        //            .ToListAsync();

        //        var bookings = await BookingServices
        //            .Find(b => b.BookingServiceAt >= startDate && b.BookingServiceAt <= endDate && b.IsDeleted != true)
        //            .ToListAsync();


        //        // tạo danh sách các thành viên với thông tin hoạt động
        //        var memberActivity = new List<MemberActivityResponseDTO>();

        //        // lấy các bài viết của từng thành viên
        //        var postGroupedByAccount = posts
        //            .GroupBy(p => p.AccId)
        //            .ToDictionary(g => g.Key, g => g.Count());

        //        // lấy các bình luận của từng thành viên
        //        var commentGroupedByAccount = comments
        //            .GroupBy(c => c.AccId)
        //            .ToDictionary(g => g.Key, g => g.Count());

        //        // lấy các dịch vụ đã đặt của từng thành viên
        //        var bookingGroupedByAccount = bookings
        //            .GroupBy(b => b.AccId)
        //            .ToDictionary(g => g.Key, g => g.Count());

        //        // tính số lần thanh toán của mỗi thành viên
        //        var paymentGroupedByAccount = bookings
        //            .Where(b => b.FirstPaymentAt != null || b.SecondPaymentAt != null)
        //            .GroupBy(b => b.AccId)
        //            .ToDictionary(g => g.Key, g => g.Count());


        //        var accountIds = postGroupedByAccount.Keys
        // .Concat(commentGroupedByAccount.Keys)
        // .Concat(bookingGroupedByAccount.Keys)
        // .Distinct()
        // .ToList();

        //        var accounts = await _Account
        //            .Find(a => accountIds.Contains(a.AccId))
        //            .ToListAsync();

        //        var roleIds = accounts
        //.Where(a => !string.IsNullOrEmpty(a.RoleId))
        //.Select(a => a.RoleId)
        //.Distinct()
        //        .ToList();

        //        var roles = await _Role
        //            .Find(r => roleIds.Contains(r.RoleId))
        //            .ToListAsync();


        //        foreach (var account in postGroupedByAccount)
        //        {
        //            var accountId = account.Key;
        //            var totalPosts = account.Value;
        //            var totalComments = commentGroupedByAccount.ContainsKey(accountId) ? commentGroupedByAccount[accountId] : 0;
        //            var totalBookings = bookingGroupedByAccount.ContainsKey(accountId) ? bookingGroupedByAccount[accountId] : 0;
        //            var totalPayments = paymentGroupedByAccount.ContainsKey(accountId) ? paymentGroupedByAccount[accountId] : 0;
        //            var accountInfo = accounts.FirstOrDefault(a => a.AccId == accountId);
        //            var roleInfo = roles.FirstOrDefault(r => r.RoleId == accountInfo?.RoleId);


        //            var memberDTO = new MemberActivityResponseDTO
        //            {
        //                AccId = accountId,
        //                AccountName = accountInfo?.FullName,
        //                AccountAddress = accountInfo?.Address,
        //                RoleName = roleInfo?.RoleName,
        //                TotalPosts = totalPosts,
        //                TotalComments = totalComments,
        //                TotalBookings = totalBookings,
        //                TotalPayments = totalPayments,
        //                TotalActivity = totalPosts + totalComments + totalBookings + totalPayments // Tổng hoạt động
        //            };

        //            memberActivity.Add(memberDTO);
        //        }

        //        return memberActivity.OrderByDescending(m => m.TotalActivity).Take(10).ToList();

        //    }


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
                .ToDictionary(g => $"Tháng {g.Key}", g => g.Count());
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


        //public async Task<Dictionary<string, int>> GetMostBookedServicesByExpertAsync(string accId)
        //{
        //    //Lấy tất cả các service của Expert
        //    var expertServiceFilter = Builders<Service>.Filter.Eq(s => s.ProviderId, accId);
        //    var expertServices = await Services.Find(expertServiceFilter).ToListAsync();

        //    var expertServiceObjectIds = expertServices.Select(s => new ObjectId(s.ServiceId)).ToList();


        //    var serviceIdNameDict = expertServices.ToDictionary(s => s.ServiceId, s => s.ServiceName);


        //    var expertServiceIds = serviceIdNameDict.Keys.ToList();

        //    //Lọc BookingService có ServiceId thuộc danh sách trên và Status là Accepted
        //    var bookingFilter = Builders<BookingService>.Filter.And(
        //        Builders<BookingService>.Filter.In(b => b.ServiceId, expertServiceIds),
        //        Builders<BookingService>.Filter.Eq(b => b.BookingServiceStatus, "Accept") 
        //    );

        //    var bookings = await BookingServices.Find(bookingFilter).ToListAsync();

        //    //Nhóm theo ServiceId và đếm
        //    var result = bookings
        //        .GroupBy(b => b.ServiceId)
        //        .ToDictionary(
        //            g => serviceIdNameDict.TryGetValue(g.Key, out var name) ? name : "Unknown",
        //            g => g.Count()
        //        );

        //    return result;
        //}
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
