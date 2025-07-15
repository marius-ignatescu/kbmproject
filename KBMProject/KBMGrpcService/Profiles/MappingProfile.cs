using AutoMapper;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("o") : string.Empty))
                .ReverseMap();

            CreateMap<UpdateUserRequest, User>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<CreateUserRequest, User>();

            CreateMap<CreateOrganizationRequest, Organization>();
            CreateMap<Organization, CreateOrganizationResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrganizationId));

            CreateMap<Organization, OrganizationResponse>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.HasValue ? src.UpdatedAt.Value.ToString("o") : string.Empty))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrganizationId));

            CreateMap<UpdateOrganizationRequest, Organization>()
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
