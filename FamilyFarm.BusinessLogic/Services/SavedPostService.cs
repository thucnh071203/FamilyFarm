using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class SavedPostService : ISavedPostService
    {
        private readonly ISavedPostRepository _savedPostRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public SavedPostService(ISavedPostRepository savedPostRepository, IAccountRepository accountRepository, IPostRepository postRepository, IMapper mapper)
        {
            _savedPostRepository = savedPostRepository;
            _accountRepository = accountRepository;
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<CreatedSavedPostResponseDTO?> SavedPost(string? accId, CreateSavedPostRequestDTO request)
        {
            if(string.IsNullOrEmpty(accId))
                return null;

            if(request == null || request.PostId == null)
                return null;

            var account = await _accountRepository.GetAccountById(accId);
            if (account == null)
                return new CreatedSavedPostResponseDTO
                {
                    Message = "Not found this account.",
                    Success = false
                };

            var savedPostRequest = new SavedPost
            {
                SavedPostId = "", //Để rỗng do trong DAO có tự tạo lại ID
                AccId = accId,
                PostId = request.PostId
            };

            var savedPost = await _savedPostRepository.CreateSavedPost(savedPostRequest);

            if(savedPost == null) 
                return new CreatedSavedPostResponseDTO
                {
                    Message = "An error occurred during execution.",
                    Success = false
                };

            return new CreatedSavedPostResponseDTO
            {
                Message = "Saved post successfully.",
                Success = true,
                Data = new Models.Mapper.SavedPostMapper
                {
                    Post = await _postRepository.GetPostById(savedPost.PostId),
                    Account = _mapper.Map<MyProfileDTO>(await _accountRepository.GetAccountById(savedPost.AccId)),
                    SavedAt = savedPost.SavedAt
                }
            };
        }
    }
}
