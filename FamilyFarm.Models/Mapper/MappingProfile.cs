using AutoMapper;
using FamilyFarm.Models.DTOs.EntityDTO;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    // Class nay de AutoMapper
    // Co the them cac AutoMapper khac
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SendMessageRequestDTO, ChatDetail>();

            CreateMap<ChatDetail, SendMessageResponseDTO>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));

            CreateMap<CommentRequestDTO, Comment>();

            CreateMap<Comment, CommentResponseDTO>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src));

            CreateMap<SendNotificationRequestDTO, Notification>()
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
             .ForMember(dest => dest.CategoryNotifiId, opt => opt.MapFrom(src => src.CategoryNotiId));

            CreateMap<Notification, SendNotificationResponseDTO>();
            CreateMap<Account, MyProfileDTO>();
            CreateMap<Account, MiniAccountDTO>();
            CreateMap<Chat, ChatDTO>();
            CreateMap<Chat, ChatDTO>()
                .ForMember(dest => dest.ChatId, opt => opt.MapFrom(src => src.ChatId))
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageAccId, opt => opt.Ignore())
                .ForMember(dest => dest.Receiver, opt => opt.Ignore());
            CreateMap<Account, MyProfileDTO>()
                .ForMember(dest => dest.AccId, opt => opt.MapFrom(src => src.AccId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar));

            CreateMap<Notification, NotificationDTO>()
                .ForMember(dest => dest.NotifiId, opt => opt.MapFrom(src => src.NotifiId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.CategoryNotifiId, opt => opt.MapFrom(src => src.CategoryNotifiId))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.TargetId, opt => opt.MapFrom(src => src.TargetId))
                .ForMember(dest => dest.TargetType, opt => opt.MapFrom(src => src.TargetType))
                .ForMember(dest => dest.SenderName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderAvatar, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
                .ForMember(dest => dest.TargetContent, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<Review, ReviewRequestDTO>();
        }
    }
}
