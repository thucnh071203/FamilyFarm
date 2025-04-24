using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDAO _commentDAO;

        public CommentRepository(CommentDAO commentDAO)
        {
            _commentDAO = commentDAO;
        }

        public async Task<List<Comment>> GetAllByPost(string postId)
        {
            return await _commentDAO.GetAllByPostAsync(postId);
        }

        public async Task<Comment> GetById(string id)
        {
            return await _commentDAO.GetByIdAsync(id);
        }

        public async Task<Comment> Create(Comment comment)
        {
            return await _commentDAO.CreateAsync(comment);
        }

        public async Task<Comment> Update(string id, Comment comment)
        {
            return await _commentDAO.UpdateAsync(id, comment);
        }
        public async Task Delete(string id)
        {
            await _commentDAO.DeleteAsync(id);
        }

    }
}
