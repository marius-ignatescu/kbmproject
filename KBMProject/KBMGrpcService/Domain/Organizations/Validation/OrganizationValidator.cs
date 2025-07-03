using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Organizations.Validation
{
    public static class OrganizationValidator
    {
        public static async Task ValidateCreateOrganizationRequest(CreateOrganizationRequest request, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name is required"));

            if (!string.IsNullOrEmpty(request.Name))
            {
                var organizationExists = await OrganizationRepositoryHelper.NameExistsAsync(db, request.Name);

                if (organizationExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "This organization already exists"));
            }
        }

        public static async Task ValidateOrganizationUpdateRequest(UpdateOrganizationRequest request, Organization existingOrganization, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Organization name is required"));

            if (!string.IsNullOrEmpty(request.Name))
            {
                var organizationExists = await OrganizationRepositoryHelper.OrganizationExistsByNameAsync(db, request.Name, existingOrganization.OrganizationId);

                if (organizationExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Update failed. This organization already exists"));
            }
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
