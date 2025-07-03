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
    }
}
