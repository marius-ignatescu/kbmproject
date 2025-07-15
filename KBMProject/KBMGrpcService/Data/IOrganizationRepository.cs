using KBMGrpcService.Models;

namespace KBMGrpcService.Data
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(int id);
        Task<IEnumerable<Organization>> GetAllAsync();
        Task AddAsync(Organization organization);
        void Update(Organization organization);
        void Delete(Organization organization);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name);
        Task<bool> NameExistsAsync(string name, int currentOrganizationId);
        bool SaveChanges();
    }
}
