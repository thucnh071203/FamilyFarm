using FamilyFarm.DataAccess.Context;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Repositories;
using FamilyFarm.BusinessLogic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FamilyFarm.BusinessLogic.Services;
using MongoDB.Driver;
using FamilyFarm.BusinessLogic.PasswordHashing;
using Microsoft.OpenApi.Models;
using FamilyFarm.BusinessLogic.Config;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Repositories.Interfaces;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.BusinessLogic.Hubs;
using Microsoft.AspNetCore.SignalR;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.ModelsConfig;
using Microsoft.AspNetCore.Mvc;
using FamilyFarm.Models.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.Configure<MongoDBSetting>(
//    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<MongoDBContext>();
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var context = sp.GetRequiredService<MongoDBContext>();
    return context.Database!;
});

// DAO DI
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<RoleDAO>();
builder.Services.AddScoped<AccountDAO>();
builder.Services.AddScoped<CommentDAO>();
builder.Services.AddScoped<CategoryReactionDAO>();
builder.Services.AddScoped<ReactionDAO>();
builder.Services.AddScoped<ReportDAO>();
builder.Services.AddScoped<GroupDAO>();
builder.Services.AddScoped<PostDAO>();
builder.Services.AddScoped<FriendRequestDAO>();
builder.Services.AddScoped<GroupRoleDAO>();
builder.Services.AddScoped<GroupMemberDAO>();
builder.Services.AddScoped<CategoryPostDAO>();
builder.Services.AddScoped<HashtagDAO>();
builder.Services.AddScoped<PostCategoryDAO>();
builder.Services.AddScoped<PostImageDAO>();
builder.Services.AddScoped<PostTagDAO>();
builder.Services.AddScoped<FriendDAO>();
builder.Services.AddScoped<BookingServiceDAO>();
builder.Services.AddScoped<ServiceDAO>();
builder.Services.AddScoped<RoleInGroupDAO>();
builder.Services.AddScoped<SearchHistoryDAO>();
builder.Services.AddScoped<ChatDetailDAO>();
builder.Services.AddScoped<ChatDAO>();
builder.Services.AddScoped<ServiceDAO>();
builder.Services.AddScoped<CategoryServiceDAO>();
builder.Services.AddScoped<NotificationDAO>();
builder.Services.AddScoped<NotificationStatusDAO>();
builder.Services.AddScoped<ProcessDAO>();
builder.Services.AddScoped<SharePostDAO>();
builder.Services.AddScoped<SharePostTagDAO>();
builder.Services.AddScoped<CategoryNotificationDAO>();
builder.Services.AddScoped<StatisticDAO>();
builder.Services.AddScoped<SavedPostDAO>();
builder.Services.AddScoped<ProcessStepDAO>();
builder.Services.AddScoped<ProcessStepImageDAO>();
builder.Services.AddScoped<PaymentDAO>();
builder.Services.AddScoped<RevenueDAO>();
builder.Services.AddScoped<ReviewDAO>();

// Repository DI
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICategoryReactionRepository, CategoryReactionRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
builder.Services.AddScoped<IGroupRoleRepository, GroupRoleRepository>();
builder.Services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
builder.Services.AddScoped<ICategoryPostRepository, CategoryPostRepository>();
builder.Services.AddScoped<IHashTagRepository, HashTagRepository>();
builder.Services.AddScoped<IPostCategoryRepository, PostCategoryRepository>();
builder.Services.AddScoped<IPostImageRepository, PostImageRepository>();
builder.Services.AddScoped<IPostTagRepository, PostTagRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IBookingServiceRepository, BookingServiceRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IRoleInGroupRepository, RoleInGroupRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatDetailRepository, ChatDetailRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ICategoryServiceRepository, CategoryServiceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationStatusRepository, NotificationStatusRepository>();
builder.Services.AddScoped<IProcessRepository, ProcessRepository>();
builder.Services.AddScoped<ISharePostRepository, SharePostRepository>();
builder.Services.AddScoped<ISharePostTagRepository, SharePostTagRepository>();
builder.Services.AddScoped<ICategoryNotificationRepository, CategoryNotificationRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<ISavedPostRepository, SavedPostRepository>();
builder.Services.AddScoped<IProcessStepRepository, ProcessStepRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRevenueRepository, RevenueRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Service DI
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<IGroupRoleService, GroupRoleService>();
builder.Services.AddScoped<IGroupMemberService, GroupMemberService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IBookingServiceService, BookingServiceService>();
builder.Services.AddScoped<IRoleInGroupService, RoleInGroupService>();
builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<IServicingService, ServicingService>();
builder.Services.AddScoped<ICategoryServicingService, CategoryServicingService>();
builder.Services.AddScoped<ICategoryPostService, CategoryPostService>();
builder.Services.AddScoped<ICategoryReactionService, CategoryReactionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICohereService, CohereService>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddScoped<ISharePostService, SharePostService>();
builder.Services.AddSingleton<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<ISavedPostService, SavedPostService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRepaymentCacheService, RepaymentCacheService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// ⚠️ Thêm dòng này để tránh lỗi license
QuestPDF.Settings.License = LicenseType.Community;

//builder.Services.AddScoped<FirebaseConnection>();

// Cấu hình CORS nếu cần (cho phép client kết nối SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://127.0.0.1:5500", "https://localhost:7218") // thay bằng đúng origin đang test
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // khi dùng SignalR
    });
});

builder.Services.Configure<VNPayConfig>(builder.Configuration.GetSection("VNPay"));

builder.Services.AddSignalR();

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();

//SECURITY
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = tokenValidationParameters;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => new {
                Field = e.Key,
                Errors = e.Value.Errors.Select(er => er.ErrorMessage).ToArray()
            });

        var result = new BadRequestObjectResult(errors);
        return result;
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer {token}' vào ô bên dưới (không có dấu ngoặc kép)",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    await next();
});

app.UseHttpsRedirection();
app.UseCors("AllowAll"); 
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<TopEngagedPostHub>("/topEngagedPostHub");
app.MapHub<CategoryServiceHub>("/categoryServiceHub");
app.MapHub<FriendHub>("/friendHub");
//app.MapHub<BookingHub>("/chatHub");

app.Run();  
