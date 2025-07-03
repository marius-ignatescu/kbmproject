using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Organizations.Extensions
{
    public static class OrganizationExtensions
    {
        /// <summary>
        /// Update Organization entity from a Organization request
        /// </summary>
        /// <param name="organization">The Organization entity to be updated</param>
        /// <param name="request">The organization request input</param>
        public static void UpdateFromRequest(this Organization organization, UpdateOrganizationRequest request)
        {
            organization.Name = request.Name ?? organization.Name;
            organization.Address = request.Address ?? organization.Address;
            organization.UpdatedAt = DateTime.UtcNow;
        }
    }
}
