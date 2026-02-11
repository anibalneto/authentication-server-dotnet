using AutoMapper;
using Auth.Application.DTOs.Admin;
using Auth.Application.DTOs.User;
using Auth.Domain.Entities;

namespace Auth.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserProfileResponse>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));

        CreateMap<User, UserDetailResponse>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));
    }
}
