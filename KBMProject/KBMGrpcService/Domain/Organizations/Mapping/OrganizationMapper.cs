using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Organizations.Mapping
{
    /// <summary>
    /// Helper class used to map entities, requests and responses
    /// </summary>
    public static class OrganizationMapper
    {
        /// <summary>
        /// Creates a organization response based on a Organization entity
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        public static OrganizationResponse MapToOrganizationResponse(Organization organization)
        {
            return new OrganizationResponse
            {
                Id = organization.OrganizationId,
                Name = organization.Name,
                Address = organization.Address ?? string.Empty,
                CreatedAt = organization.CreatedAt.ToString(),
                UpdatedAt = organization.UpdatedAt?.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Creates a Organization entity from a CreateOrganizationRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Organization CreateOrganizationEntity(CreateOrganizationRequest request)
        {
            return new Organization
            {
                Name = request.Name,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
