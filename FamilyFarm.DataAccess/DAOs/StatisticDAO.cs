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
            Comments = database.GetCollection<Comment>("Comment");
            Services = database.GetCollection<Service>("Service");
            CategoryServices = database.GetCollection<CategoryService>("CategoryService");
            _Role = database.GetCollection<Role>("Role");
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


        public async Task<Dictionary<string, List<BookingServiceByStatusDTO>>> CountByStatusAsync(string accId)
        {
            var bookingFilter = Builders<BookingService>.Filter.Eq(x => x.AccId, accId);
            var bookings = await BookingServices.Find(bookingFilter).ToListAsync();

            var serviceIds = bookings.Select(b => b.ServiceId).Distinct().ToList();
            var serviceFilter = Builders<Service>.Filter.In(s => s.ServiceId, serviceIds);
            var services = await Services.Find(serviceFilter).ToListAsync();

            var result = bookings.Select(b =>
            {
                var service = services.FirstOrDefault(s => s.ServiceId == b.ServiceId);

                return new BookingServiceByStatusDTO
                {
                    BookingServiceId = b.BookingServiceId,
                    AccId = b.AccId,
                    ServiceId = b.ServiceId,
                    Price = b.Price,
                    BookingServiceAt = b.BookingServiceAt,
                    BookingServiceStatus = b.BookingServiceStatus,
                    CancelServiceAt = b.CancelServiceAt,
                    RejectServiceAt = b.RejectServiceAt,
                    FirstPayment = b.FirstPayment,
                    FirstPaymentAt = b.FirstPaymentAt,
                    SecondPayment = b.SecondPayment,
                    SecondPaymentAt = b.SecondPaymentAt,
                    IsDeleted = b.IsDeleted,

                    ServiceName = service?.ServiceName,
                    ServiceDescription = service?.ServiceDescription
                };
            }).ToList();

            return result
                .GroupBy(x => x.BookingServiceStatus ?? "UNKNOWN")
                .ToDictionary(g => g.Key, g => g.ToList());
        }

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
