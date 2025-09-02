using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.WebApi.DTOs.LearningObjects;
using AutoMapper;

namespace AtomicLmsCore.WebApi.Mappings;

/// <summary>
///     AutoMapper profile for LearningObject entity mappings.
/// </summary>
public class LearningObjectMappingProfile : Profile
{
    public LearningObjectMappingProfile()
    {
        CreateMap<LearningObject, LearningObjectDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        CreateMap<LearningObject, LearningObjectListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // Note: We don't map DTOs to Commands as this would create tight coupling
        // between Web API and Application layers. Controllers should construct commands directly.
    }
}
