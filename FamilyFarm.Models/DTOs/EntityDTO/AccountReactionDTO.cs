using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.DTOs.EntityDTO
{
    public class AccountReactionDTO
    {
        public Reaction? Reaction { get; set; }
        public MyProfileDTO? AccountOfReaction { get; set; } //Chỉ trả về thông tin profile, không trả về các thông tin nhạy cảm khác như password, hoặc refresh token
        public CategoryReaction? CategoryReaction { get; set; } //Thể loại của reaction như là ảnh và tên cụ thể của reaction
    }
}
