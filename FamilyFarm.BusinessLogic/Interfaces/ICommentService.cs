using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Interfaces
{
    public interface ICommentService
    {
        Task<List<Comment>> GetAllByPost(string postId);
        Task<Comment> GetById(string id);
        Task<Comment> Create(Comment comment);
        Task<Comment> Update(string id, Comment comment);
        Task Delete(string id);
    }
}
