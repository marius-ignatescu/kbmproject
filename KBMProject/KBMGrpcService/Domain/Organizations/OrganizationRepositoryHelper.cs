using KBMGrpcService.Data;
using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Domain.Organizations
{
    /// <summary>
    /// Helper class for database queries or utility access logic
    /// </summary>
    public static class OrganizationRepositoryHelper
    {
        /// <summary>
        /// Gets user by ID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<Organization?> GetActiveOrganizationByIdAsync(AppDbContext db, int organizationId)
        {
            return await db.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId && o.DeletedAt == null);
        }

        /// <summary>
        /// Gets user by ID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public static async Task<Organization?> GetActiveOrganizationByIdWithNoTrackingAsync(AppDbContext db, int organizationId)
        {
            return await db.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.OrganizationId == organizationId && u.DeletedAt == null);
        }

        /// <summary>
        /// Gets active organizations
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IQueryable<Organization> GetActiveOrganizations(AppDbContext db)
        {
            return db.Organizations.AsNoTracking().Where(u => u.DeletedAt == null);
        }

        /// <summary>
        /// Check if the organization exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<bool> NameExistsAsync(AppDbContext db, string name)
        {
            return await db.Organizations
                .AnyAsync(o => o.Name == name && o.DeletedAt == null);
        }

        /// <summary>
        /// Check if another organization with the same name exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <param name="existingOrganizationId"></param>
        /// <returns></returns>
        public static async Task<bool> OrganizationExistsByNameAsync(AppDbContext db, string name, int existingOrganizationId)
        {
            return await db.Organizations.AnyAsync(u =>
                u.Name == name && u.OrganizationId != existingOrganizationId && u.DeletedAt == null);
        }
    }
}
