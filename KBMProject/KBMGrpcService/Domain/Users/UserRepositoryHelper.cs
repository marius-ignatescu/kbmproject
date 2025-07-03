using KBMGrpcService.Data;
using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Domain.Users
{
    /// <summary>
    /// Helper class for database queries or utility access logic
    /// </summary>
    public static class UserRepositoryHelper
    {
        /// <summary>
        /// Gets user by ID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<User?> GetActiveUserByIdAsync(AppDbContext db, int userId)
        {
            return await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId && u.DeletedAt == null);
        }

        /// <summary>
        /// Gets active users
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IQueryable<User> GetActiveUsers(AppDbContext db)
        {
            return db.Users.AsNoTracking().Where(u => u.DeletedAt == null);
        }

        /// <summary>
        /// Check if another user with the same email exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="email"></param>
        /// <param name="existingUserId"></param>
        /// <returns></returns>
        public static async Task<bool> UserExistsByEmailAsync(AppDbContext db, string email, int existingUserId)
        {
            return await db.Users.AnyAsync(u =>
                    u.Email == email && u.UserId != existingUserId && u.DeletedAt == null);
        }

        /// <summary>
        /// Check if another user with the same name exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="username"></param>
        /// <param name="existingUserId"></param>
        /// <returns></returns>
        public static async Task<bool> UserExistsByUsernameAsync(AppDbContext db, string username, int existingUserId)
        {
            return await db.Users.AnyAsync(u =>
                    u.Username == username && u.UserId != existingUserId && u.DeletedAt == null);
        }
    }
}
