using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Data
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _db;

        public OrganizationRepository(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get organization by specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Organization?> GetByIdAsync(int id)
        {
            return await _db.Organizations.FirstOrDefaultAsync(u => u.OrganizationId == id && u.DeletedAt == null);
        }

        /// <summary>
        /// Get all active organizations
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _db.Organizations.AsNoTracking().Where(u => u.DeletedAt == null).ToListAsync();
        }

        /// <summary>
        /// Add organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        public async Task AddAsync(Organization organization)
        {
            await _db.Organizations.AddAsync(organization);
        }

        /// <summary>
        /// Update organization
        /// </summary>
        /// <param name="organization"></param>
        public void Update(Organization organization)
        {
            organization.UpdatedAt = DateTime.UtcNow;
            _db.Organizations.Update(organization);
        }

        /// <summary>
        /// Delete organization
        /// </summary>
        /// <param name="organization"></param>
        public void Delete(Organization organization)
        {
            organization.DeletedAt = DateTime.UtcNow;
            _db.Organizations.Update(organization);
        }

        /// <summary>
        /// Check if the organization exists by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Organizations.AnyAsync(o => o.OrganizationId == id && o.DeletedAt == null);
        }

        /// <summary>
        /// Check if the organization exists by name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> NameExistsAsync(string name)
        {
            return await _db.Organizations.AnyAsync(o => o.Name == name && o.DeletedAt == null);
        }

        /// <summary>
        /// Check if the organization exists by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="currentOrganizationId"></param>
        /// <returns></returns>
        public async Task<bool> NameExistsAsync(string name, int currentOrganizationId)
        {
            return await _db.Organizations.AnyAsync(u =>
                u.Name == name && u.OrganizationId != currentOrganizationId && u.DeletedAt == null);
        }

        public bool SaveChanges()
        {
            return (_db.SaveChanges() >= 0);
        }
    }
}
