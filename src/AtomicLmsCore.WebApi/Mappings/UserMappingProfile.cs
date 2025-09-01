using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.WebApi.DTOs.Users;
using AutoMapper;

namespace AtomicLmsCore.WebApi.Mappings;

/// <summary>
///     AutoMapper profile for User entity mappings.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.Tenant.Id));

        CreateMap<User, UserListDto>();
    }
}
