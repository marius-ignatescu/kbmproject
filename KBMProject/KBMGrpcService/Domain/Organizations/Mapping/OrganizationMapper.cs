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
