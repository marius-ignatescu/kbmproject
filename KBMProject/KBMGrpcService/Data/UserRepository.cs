using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == id && u.DeletedAt == null);
        }

        /// <summary>
        /// Add User
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        public void Update(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="user"></param>
        public void Delete(User user)
        {
            user.DeletedAt = DateTime.UtcNow;
            _db.Users.Update(user);
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _db.Users.AsNoTracking().Where(u => u.DeletedAt == null).ToListAsync();
        }

        /// <summary>
        /// Get users by organization
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetUsersByOrganization(int orgId)
        {
            return await _db.Users.Where(u => u.DeletedAt == null && u.OrganizationId == orgId).ToListAsync();
        }

        /// <summary>
        /// Check if user name exists
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> UsernameExistsAsync(string userName)
        {
            return await _db.Users.AnyAsync(o => o.Username == userName && o.DeletedAt == null);
        }

        /// <summary>
        /// Check if another user with this email exists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(o => o.Email == email && o.DeletedAt == null);
        }

        /// <summary>
        /// Check if another user with the same email exists
        /// </summary>
        /// <param name="email"></param>
        /// <param name="existingUserId"></param>
        /// <returns></returns>
        public async Task<bool> UserExistsByEmailAsync(string email, int existingUserId)
        {
            return await _db.Users.AnyAsync(u => u.Email == email && u.UserId != existingUserId && u.DeletedAt == null);
        }

        /// <summary>
        /// Check if another user with the same name exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="username"></param>
        /// <param name="existingUserId"></param>
        /// <returns></returns>
        public async Task<bool> UserExistsByUsernameAsync(string username, int existingUserId)
        {
            return await _db.Users.AnyAsync(u => u.Username == username && u.UserId != existingUserId && u.DeletedAt == null);
        }

        /// <summary>
        /// Save changes
        /// </summary>
        /// <returns></returns>
        public bool SaveChanges()
        {
            return (_db.SaveChanges() >= 0);
        }
    }
}
