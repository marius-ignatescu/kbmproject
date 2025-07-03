using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Users.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Update User entity from a User request
        /// </summary>
        /// <param name="user">The User entity to be updated</param>
        /// <param name="request">The user request input</param>
        public static void UpdateFromRequest(this User user, UpdateUserRequest request)
        {
            user.Name = request.Name ?? user.Name;
            user.Username = request.Username ?? user.Username;
            user.Email = request.Email ?? user.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Update User entity from a User request
        /// </summary>
        /// <param name="user">The User entity to be updated</param>
        /// <param name="request">The user request input</param>
        public static void UpdateAssociationFromRequest(this User user, AssociationRequest request)
        {
            user.UpdatedAt = DateTime.UtcNow;
            user.OrganizationId = request.OrganizationId;
        }

        /// <summary>
        /// Soft deletes the specified user
        /// </summary>
        /// <param name="user">The user to be deleted</param>
        /// <param name="db">Db Context</param>
        /// <returns></returns>
        public static async Task<bool> SoftDelete(this User user, AppDbContext db)
        {
            user.DeletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }
    }
}
