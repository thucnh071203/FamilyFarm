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
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<RoleDAO>();
builder.Services.AddScoped<AccountDAO>();
builder.Services.AddScoped<CommentDAO>();
builder.Services.AddScoped<CategoryReactionDAO>();
builder.Services.AddScoped<ReactionPostDAO>();
builder.Services.AddScoped<ReportDAO>();

// Repository DI
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICategoryReactionRepository, CategoryReactionRepository>();
builder.Services.AddScoped<IReactionPostRepository, ReactionPostRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Service DI
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionPostService, ReactionPostService>();
builder.Services.AddScoped<IReactionPostService, ReactionPostService>();
builder.Services.AddScoped<IReportService, ReportService>();
//builder.Services.AddScoped<FirebaseConnection>();

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

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
