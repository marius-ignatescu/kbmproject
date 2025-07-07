using KBMGrpcService.Data;
using KBMGrpcService.Domain.Organizations;
using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Tests.Organizations
{
    public class OrganizationRepositoryHelperTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetActiveOrganizationByIdAsync_ReturnsActiveOrg()
        {
            using var context = GetInMemoryDbContext();
            var org = new Organization { OrganizationId = 1, Name = "TestOrg", DeletedAt = null };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            var result = await OrganizationRepositoryHelper.GetActiveOrganizationByIdAsync(context, 1);

            Assert.NotNull(result);
            Assert.Equal("TestOrg", result.Name);
        }

        [Fact]
        public async Task GetActiveOrganizationByIdAsync_ReturnsNull_WhenDeleted()
        {
            using var context = GetInMemoryDbContext();
            var org = new Organization { OrganizationId = 1, Name = "DeletedOrg", DeletedAt = DateTime.UtcNow };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            var result = await OrganizationRepositoryHelper.GetActiveOrganizationByIdAsync(context, 1);

            Assert.Null(result);
        }

        [Fact]
        public void GetActiveOrganizations_ReturnsOnlyNonDeleted()
        {
            using var context = GetInMemoryDbContext();
            context.Organizations.Add(new Organization { OrganizationId = 1, Name = "OrganizationOne", DeletedAt = null });
            context.Organizations.Add(new Organization { OrganizationId = 2, Name = "OrganizationTwo", DeletedAt = DateTime.UtcNow });
            context.SaveChanges();

            var result = OrganizationRepositoryHelper.GetActiveOrganizations(context).ToList();

            Assert.Single(result);
            Assert.Equal("OrganizationOne", result[0].Name);
        }

        [Fact]
        public async Task NameExistsAsync_ReturnsTrue_WhenNameExists()
        {
            using var context = GetInMemoryDbContext();
            context.Organizations.Add(new Organization { Name = "ExistingOrg" });
            await context.SaveChangesAsync();

            var exists = await OrganizationRepositoryHelper.NameExistsAsync(context, "ExistingOrg");

            Assert.True(exists);
        }

        [Fact]
        public async Task OrganizationExistsByNameAsync_ReturnsTrue_WhenOtherOrgHasName()
        {
            using var context = GetInMemoryDbContext();
            context.Organizations.Add(new Organization { OrganizationId = 1, Name = "First Organization" });
            context.Organizations.Add(new Organization { OrganizationId = 2, Name = "Second Organization" });
            await context.SaveChangesAsync();

            var exists = await OrganizationRepositoryHelper.OrganizationExistsByNameAsync(context, "First Organization", 2);

            Assert.True(exists);
        }

        [Fact]
        public async Task OrganizationExistsByNameAsync_ReturnsFalse_WhenSameOrg()
        {
            using var context = GetInMemoryDbContext();
            context.Organizations.Add(new Organization { OrganizationId = 1, Name = "Organization" });
            await context.SaveChangesAsync();

            var exists = await OrganizationRepositoryHelper.OrganizationExistsByNameAsync(context, "Organization", 1);

            Assert.False(exists);
        }
    }
}
