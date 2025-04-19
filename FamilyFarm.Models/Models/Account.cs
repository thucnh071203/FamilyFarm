using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId AccId { get; set; }
        [BsonRequired]
        public required ObjectId RoleId { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public required string Gender { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public string? IdentifierNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Background { get; set; }
        public string? Certificate { get; set; }
        public string? WorkAt { get; set; }
        public string? StudyAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public int? FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int Status { get; set; }
        public int? Otp {  get; set; }
        public DateTime? CreateOtp { get; set; }
    }
}
