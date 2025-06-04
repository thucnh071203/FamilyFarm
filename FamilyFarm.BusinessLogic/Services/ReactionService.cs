using AutoMapper;
using FamilyFarm.BusinessLogic.Hubs;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ReactionService : IReactionService
    {
        private readonly IReactionRepository _reactionRepository;
        private readonly ICategoryReactionRepository _categoryReactionRepository;
        private readonly IHubContext<TopEngagedPostHub> _hubContext;
        private readonly IStatisticService _statisticService;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public ReactionService(IReactionRepository reactionRepository, ICategoryReactionRepository categoryReactionRepository, IHubContext<TopEngagedPostHub> hubContext, IStatisticService statisticService, IAccountRepository accountRepository, IMapper mapper)
        {
            _reactionRepository = reactionRepository;
            _categoryReactionRepository = categoryReactionRepository;
            _hubContext = hubContext;
            _statisticService = statisticService;
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        public async Task<bool> ToggleReactionAsync(string entityId, string entityType, string accId, string categoryReactionId)
        {
            // Validate ObjectId inputs
            if (!ObjectId.TryParse(entityId, out _) || !ObjectId.TryParse(accId, out _) || !ObjectId.TryParse(categoryReactionId, out _))
                return false;

            // Check if CategoryReaction exists and is not soft deleted
            var categoryReaction = await _categoryReactionRepository.GetByIdAsync(categoryReactionId);
            if (categoryReaction == null || categoryReaction.IsDeleted == true)
                return false;

            // Check if the user has any reactions to the entity
            var existingReaction = await _reactionRepository.GetByEntityAndAccAsync(entityId, entityType, accId);

            if (existingReaction == null)
            {
                // Create new reaction if it doesn't exist yet
                var newReaction = new Reaction
                {
                    ReactionId = ObjectId.GenerateNewId().ToString(),
                    EntityId = entityId,
                    EntityType = entityType,
                    AccId = accId,
                    CategoryReactionId = categoryReactionId,
                };
                await _reactionRepository.CreateAsync(newReaction);
                var updatedPosts = await _statisticService.GetTopEngagedPostsAsync(5);
                await _hubContext.Clients.All.SendAsync("topEngagedPostHub", updatedPosts);
                return true;
            }
            else
            {
                // If select same reaction, toggle IsDeleted status
                if (existingReaction.CategoryReactionId == categoryReactionId)
                {
                    if (existingReaction.IsDeleted == true)
                    {
                        var result = await _reactionRepository.RestoreAsync(existingReaction.ReactionId);
                        if (result)
                        {
                            var updatedPosts = await _statisticService.GetTopEngagedPostsAsync(5);
                            await _hubContext.Clients.All.SendAsync("topEngagedPostHub", updatedPosts);
                        }
                        return result;

                    }

                    //tao đổi tại chỗ này để nhét thêm cái thống kê vào đây
                    // logic cũ , chỉ là thêm dòng để thêm code thống kê
                    //return await _reactionRepository.RestoreAsync(existingReaction.ReactionId);


                    else
                    {
                        var result = await _reactionRepository.DeleteAsync(existingReaction.ReactionId);
                        if (result)
                        {
                            var updatedPosts = await _statisticService.GetTopEngagedPostsAsync(5);
                            await _hubContext.Clients.All.SendAsync("topEngagedPostHub", updatedPosts);
                        }
                        return result;

                    }
                    //return await _reactionRepository.DeleteAsync(existingReaction.ReactionId);
                }
                else
                {
                    // If another reaction is selected, update CategoryReactionId and set IsDeleted = false
                    return await _reactionRepository.UpdateAsync(
                        existingReaction.ReactionId,
                        categoryReactionId,
                        false
                    );
                }
            }
        }

        public async Task<ListReactionResponseDTO> GetAllByEntityAsync(string entityId, string entityType)
        {
            // Validate ObjectId input
            if (!ObjectId.TryParse(entityId, out _))
                return new ListReactionResponseDTO
                {
                    Success = false,
                    Message = "No reaction found!.",
                    AvailableCount = 0,
                    ReactionDTOs = new List<ReactionDTO>()
                };

            var reactions = await _reactionRepository.GetAllByEntityAsync(entityId, entityType);

            // Tạo danh sách DTOs
            var reactionDTOs = new List<ReactionDTO>();

            foreach (var reaction in reactions.Where(r => r.IsDeleted != true))
            {
                var categoryReaction = await _categoryReactionRepository.GetByIdAsync(reaction.CategoryReactionId);
                var account = await _accountRepository.GetAccountById(reaction.AccId); // hoặc map thủ công nếu có Account model

                var dto = new ReactionDTO
                {
                    Reaction = reaction,
                    CategoryReaction = categoryReaction,
                    Account = _mapper.Map<MyProfileDTO>(account)
                };

                reactionDTOs.Add(dto);
            }

            return new ListReactionResponseDTO
            {
                Success = true,
                Message = "Get list of reactions successfully!",
                AvailableCount = reactionDTOs.Count,
                ReactionDTOs = reactionDTOs
            };

        }
    }
}