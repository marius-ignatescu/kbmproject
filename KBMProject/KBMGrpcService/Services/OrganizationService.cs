using Grpc.Core;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Services
{
    public class OrganizationService : OrganizationProtoService.OrganizationProtoServiceBase
    {
        public override async Task<CreateOrganizationResponse> CreateOrganization(CreateOrganizationRequest request, ServerCallContext context)
        {
            return new CreateOrganizationResponse();
        }

        public override async Task<OrganizationResponse> GetOrganizationById(GetOrganizationByIdRequest request, ServerCallContext context)
        {
            return new OrganizationResponse();
        }

        public override async Task<QueryOrganizationsResponse> QueryOrganizations(QueryOrganizationsRequest request, ServerCallContext context)
        {
            return new QueryOrganizationsResponse();
        }

        public override async Task<OrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context)
        {
            return new OrganizationResponse();
        }

        public override async Task<DeleteOrganizationResponse> DeleteOrganization(DeleteOrganizationRequest request, ServerCallContext context)
        {
            return new DeleteOrganizationResponse();
        }
    }
}
