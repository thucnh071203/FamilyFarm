using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<List<Comment>> GetAllByPost(string postId)
        {
            return await _commentRepository.GetAllByPost(postId);
        }

        public async Task<Comment> GetById(string id)
        {
            return await _commentRepository.GetById(id);
        }

        public async Task<Comment> Create(Comment comment)
        {
            return await _commentRepository.Create(comment);
        }

        public async Task<Comment> Update(string id, Comment comment)
        {
            return await _commentRepository.Update(id, comment);
        }
        public async Task Delete(string id)
        {
            await _commentRepository.Delete(id);
        }

    }
}
