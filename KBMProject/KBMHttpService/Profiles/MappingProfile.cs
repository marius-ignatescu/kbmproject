using AutoMapper;
using KBMContracts.Dtos;
using KBMGrpcService.Protos;

namespace KBMHttpService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Converters
            CreateMap<string?, DateTime?>().ConvertUsing<NullableDateTimeConverter>();

            // User mappings
            CreateMap<CreateUserDTO, CreateUserRequest>();
            CreateMap<UserResponse, UserDTO>();
            CreateMap<UpdateUserDTO, UpdateUserRequest>();
            CreateMap<DeleteUserDTO, DeleteUserRequest>();
            CreateMap<AssociateUserDTO, AssociationRequest>();
            CreateMap<DissociateUserDTO, DisassociationRequest>();
            CreateMap<QueryUsersRequestDTO, QueryUsersRequest>();

            // Organization mappings
            CreateMap<CreateOrganizationDTO, CreateOrganizationRequest>();
            CreateMap<OrganizationResponse, OrganizationDTO>();
            CreateMap<UpdateOrganizationDTO, UpdateOrganizationRequest>();
            CreateMap<DeleteOrganizationDTO, DeleteOrganizationRequest>();
            CreateMap<QueryOrganizationsRequestDTO, QueryOrganizationsRequest>();
        }
    }
}
