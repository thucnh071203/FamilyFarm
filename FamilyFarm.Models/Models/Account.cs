using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Models
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string AccId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
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
        public string? FacebookId { get; set; }
        public bool IsFacebook { get; set; }
        public DateTime CreatedAt { get; set; }
        // Credit card information
        public bool HasCreditCard { get; set; }
        public string? CreditNumber { get; set; }
        public string? CreditName { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
