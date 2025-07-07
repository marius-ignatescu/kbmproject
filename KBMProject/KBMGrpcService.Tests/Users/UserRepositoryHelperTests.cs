using KBMGrpcService.Data;
using KBMGrpcService.Domain.Users;
using KBMGrpcService.Models;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Tests.Users
{
    public class UserRepositoryHelperTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetActiveUserByIdAsync_ReturnsUser_WhenExistsAndNotDeleted()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { UserId = 1, DeletedAt = null };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await UserRepositoryHelper.GetActiveUserByIdAsync(context, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task GetActiveUserByIdAsync_ReturnsNull_WhenUserIsDeleted()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { UserId = 2, DeletedAt = DateTime.UtcNow };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await UserRepositoryHelper.GetActiveUserByIdAsync(context, 2);

            Assert.Null(result);
        }

        [Fact]
        public void GetActiveUsers_ReturnsOnlyNonDeletedUsers()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { UserId = 1, DeletedAt = null });
            context.Users.Add(new User { UserId = 2, DeletedAt = DateTime.UtcNow });
            context.SaveChanges();

            var result = UserRepositoryHelper.GetActiveUsers(context).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].UserId);
        }

        [Fact]
        public void GetActiveUsersByOrganization_ReturnsUsersInOrganization()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { UserId = 1, OrganizationId = 1, DeletedAt = null });
            context.Users.Add(new User { UserId = 2, OrganizationId = 2, DeletedAt = null });
            context.Users.Add(new User { UserId = 3, OrganizationId = 1, DeletedAt = DateTime.UtcNow });
            context.SaveChanges();

            var result = UserRepositoryHelper.GetActiveUsersByOrganization(context, 1).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].UserId);
        }
    }
}
