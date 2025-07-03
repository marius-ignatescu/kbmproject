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
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId && o.DeletedAt == null);
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
    }
}
