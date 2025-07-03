using Grpc.Core;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Organizations.Validation
{
    public static class OrganizationValidator
    {
        public static void ValidateCreateOrganizationRequest(CreateOrganizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));
        }

        public static void ValidatePagination(QueryOrganizationsRequest request)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }
        }
    }
}
