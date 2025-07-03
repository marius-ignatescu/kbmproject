using KBMGrpcService.Data;
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

        /// <summary>
        /// Soft deletes the specified organization
        /// </summary>
        /// <param name="user">The organization to be deleted</param>
        /// <param name="db">Db Context</param>
        /// <returns></returns>
        public static async Task<bool> SoftDelete(this Organization organization, AppDbContext db)
        {
            organization.DeletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }
    }
}
