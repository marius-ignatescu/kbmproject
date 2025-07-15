using KBMGrpcService.Models;

namespace KBMGrpcService.Data
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        void Update(User user);
        void Delete(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetUsersByOrganization(int orgId);
        Task<bool> UsernameExistsAsync(string userName);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UserExistsByEmailAsync(string email, int existingUserId);
        Task<bool> UserExistsByUsernameAsync(string username, int existingUserId);

        bool SaveChanges();
    }
}
