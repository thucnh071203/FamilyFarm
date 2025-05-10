using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.DataAccess.DAOs
{
    public class StatisticDAO
    {
        private readonly IMongoCollection<Post> Posts;
        private readonly IMongoCollection<PostCategory> PostCategorys;
        private readonly IMongoCollection<Account> _Account;
        private readonly PostDAO _postDAO;
        private readonly IMongoCollection<BookingService> BookingServices;
        private readonly IMongoCollection<Comment> Comments;



        public StatisticDAO(IMongoDatabase database, PostDAO postDAO)
        {
            Posts = database.GetCollection<Post>("Post");
            PostCategorys = database.GetCollection<PostCategory>("PostCategory");
            _Account = database.GetCollection<Account>("Account");
            _postDAO = postDAO;
            BookingServices = database.GetCollection<BookingService>("BookingService");
            Comments = database.GetCollection<Comment>("Comment");

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

            var posts = await Posts
      .Find(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && !p.IsDeleted)
      .ToListAsync();

            var comments = await Comments
                .Find(c => c.CreateAt >= startDate && c.CreateAt <= endDate && (c.IsDeleted == null || c.IsDeleted == false))
                .ToListAsync();

            var bookings = await BookingServices
                .Find(b => b.BookingServiceAt >= startDate && b.BookingServiceAt <= endDate && b.IsDeleted != true)
                .ToListAsync();


            // tạo danh sách các thành viên với thông tin hoạt động
            var memberActivity = new List<MemberActivityResponseDTO>();

            // lấy các bài viết của từng thành viên
            var postGroupedByAccount = posts
                .GroupBy(p => p.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // lấy các bình luận của từng thành viên
            var commentGroupedByAccount = comments
                .GroupBy(c => c.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // lấy các dịch vụ đã đặt của từng thành viên
            var bookingGroupedByAccount = bookings
                .GroupBy(b => b.AccId)
                .ToDictionary(g => g.Key, g => g.Count());

            // tính số lần thanh toán của mỗi thành viên
            var paymentGroupedByAccount = bookings
                .Where(b => b.FirstPaymentAt != null || b.SecondPaymentAt != null)
                .GroupBy(b => b.AccId)
                .ToDictionary(g => g.Key, g => g.Count());


            var accountIds = postGroupedByAccount.Keys
     .Concat(commentGroupedByAccount.Keys)
     .Concat(bookingGroupedByAccount.Keys)
     .Distinct()
     .ToList();

            var accounts = await _Account
                .Find(a => accountIds.Contains(a.AccId))
                .ToListAsync();


            foreach (var account in postGroupedByAccount)
            {
                var accountId = account.Key;
                var totalPosts = account.Value;
                var totalComments = commentGroupedByAccount.ContainsKey(accountId) ? commentGroupedByAccount[accountId] : 0;
                var totalBookings = bookingGroupedByAccount.ContainsKey(accountId) ? bookingGroupedByAccount[accountId] : 0;
                var totalPayments = paymentGroupedByAccount.ContainsKey(accountId) ? paymentGroupedByAccount[accountId] : 0;
                var accountInfo = accounts.FirstOrDefault(a => a.AccId == accountId);
                var memberDTO = new MemberActivityResponseDTO
                {
                    AccId = accountId,
                    AccountName = accountInfo?.FullName,
                    AccountAddress = accountInfo?.Address,
                    TotalPosts = totalPosts,
                    TotalComments = totalComments,
                    TotalBookings = totalBookings,
                    TotalPayments = totalPayments,
                    TotalActivity = totalPosts + totalComments + totalBookings + totalPayments // Tổng hoạt động
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


    }
}
