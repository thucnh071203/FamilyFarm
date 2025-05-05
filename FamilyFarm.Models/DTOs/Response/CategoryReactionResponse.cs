using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.DTOs.Response
{



    public class CategoryReactionResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public CategoryReactionResponse(bool isSuccess, string message, T? data = default)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }
    }

}
