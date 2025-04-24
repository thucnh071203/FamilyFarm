using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{
    public class RegisterExpertReponseDTO
    {
        public bool IsSuccess { get; set; }             // true if register successful
        public string MessageError { get; set; }           // Message if has error
        
    }
}
