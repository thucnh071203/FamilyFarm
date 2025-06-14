﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamilyFarm.BusinessLogic.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMemberRepository _memberRepository;
        private readonly IUploadFileService _uploadFileService;

        public GroupService(IGroupRepository groupRepository, IGroupMemberRepository memberRepository, IUploadFileService uploadFileService)
        {
            _groupRepository = groupRepository;
            _memberRepository = memberRepository;
            _uploadFileService = uploadFileService;
        }

        public async Task<GroupResponseDTO> GetAllGroup()
        {
            var listAllGroup = await _groupRepository.GetAllGroup();

            if (listAllGroup.Count == 0 || listAllGroup == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group list is empty"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get all group successfully",
                Count = listAllGroup.Count,
                Data = listAllGroup
            };
        }

        public async Task<List<Group>> GetAllByUserId(string userId)
        {
            return await _groupRepository.GetAllByUserId(userId);
        }

        public async Task<GroupResponseDTO> GetGroupById(string groupId)
        {
            var group = await _groupRepository.GetGroupById(groupId);

            if (group == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group not found"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Data = new List<Group> { group },
            };

        }

        public async Task<GroupResponseDTO> CreateGroup(GroupRequestDTO item)
        {
            if (item == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var bgURL = await _uploadFileService.UploadImage(item.GroupBackground);

            var avtURL = await _uploadFileService.UploadImage(item.GroupAvatar);

            var addNewGroup = new Group
            {
                GroupId = null,
                GroupName = item.GroupName,
                GroupAvatar = avtURL.UrlFile ?? "",
                GroupBackground = bgURL.UrlFile ?? "",
                PrivacyType = item.PrivacyType,
                OwnerId = item.AccountId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            };

            var created = await _groupRepository.CreateGroup(addNewGroup);

            if (created == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to create group"
                };
            }

            var getGroupId = await _groupRepository.GetLatestGroupByCreator(item.AccountId);

            if (getGroupId == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Get group id of owner failed."
                };
            }

            var addNewOwner = await _memberRepository.AddGroupOwner(getGroupId.GroupId, item.AccountId);

            if (addNewOwner == null) 
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Add owner failed."
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group created successfully",
                Data = new List<Group> { created }
            };
        }

        public async Task<GroupResponseDTO> UpdateGroup(string groupId, GroupRequestDTO item)
        {
            if (item == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var checkOwner = await _groupRepository.GetGroupById(groupId);

            if (checkOwner.OwnerId != item.AccountId)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Provider does not match"
                };
            }

            string finalBgUrl = checkOwner.GroupBackground;

            if (item.GroupBackground != null)
            {
                var imageURL = await _uploadFileService.UploadImage(item.GroupBackground);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalBgUrl = imageURL.UrlFile;
                }
            }

            string finalAvtUrl = checkOwner.GroupAvatar;

            if (item.GroupAvatar != null)
            {
                var imageURL = await _uploadFileService.UploadImage(item.GroupAvatar);
                if (!string.IsNullOrEmpty(imageURL?.UrlFile))
                {
                    finalAvtUrl = imageURL.UrlFile;
                }
            }

            var updateGroup = new Group
            {
                GroupId = null,
                GroupName = item.GroupName,
                GroupAvatar = finalAvtUrl,
                GroupBackground = finalBgUrl,
                PrivacyType = item.PrivacyType,
                OwnerId = item.AccountId,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                IsDeleted = false
            };

            var updated = await _groupRepository.UpdateGroup(groupId, updateGroup);

            if (updated == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to update group"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group updated successfully",
                Data = new List<Group> { updated }
            };
        }

        public async Task<GroupResponseDTO> GetLatestGroupByCreator(string creatorId)
        {
            var group = await _groupRepository.GetLatestGroupByCreator(creatorId);

            if (group == null)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Group not found"
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Get group successfully",
                Data = new List<Group> { group }
            };
        }

        public async Task<GroupResponseDTO> DeleteGroup(string groupId)
        {
            var deletedCount = await _groupRepository.DeleteGroup(groupId);


            if (deletedCount == 0)
            {
                return new GroupResponseDTO
                {
                    Success = false,
                    Message = "Failed to delete group"
                };
            }

            var deleteAllMember = await _memberRepository.DeleteAllGroupMember(groupId);

            if (deleteAllMember == -1)
            {
                return new GroupResponseDTO
                {
                    Success = true,
                    Message = "Delete all member failed."
                };
            }

            return new GroupResponseDTO
            {
                Success = true,
                Message = "Group deleted successfully",
                Data = null
            };
        }
    }
}
