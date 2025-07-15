using AutoMapper;
using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using KBMGrpcService.Services;
using KBMGrpcService.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Users
{
    public class UserServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateUser_CreatesUserAndReturnsUserId()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var request = new CreateUserRequest
            {
                Name = "John User",
                Username = "johnuser",
                Email = "john@gmail.com"
            };

            var serverCallContext = TestServerCallContext.Create();

            var response = await service.CreateUser(request, serverCallContext);

            Assert.NotNull(response);
            Assert.True(response.Id > 0);

            var userInDb = await context.Users.FindAsync(response.Id);
            Assert.NotNull(userInDb);
            Assert.Equal("johnuser", userInDb.Username);
            Assert.Equal("John User", userInDb.Name);
            Assert.Equal("john@gmail.com", userInDb.Email);
        }

        [Fact]
        public async Task UpdateUser_UpdatesFieldsCorrectly()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var user = new User
            {
                UserId = 1,
                Name = "Old Name",
                Username = "olduser",
                Email = "old@gmail.com",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new UpdateUserRequest
            {
                Id = 1,
                Name = "New Name",
                Username = "newuser",
                Email = "new@gmail.com"
            };

            var contextGrpc = TestServerCallContext.Create();
            var response = await service.UpdateUser(request, contextGrpc);

            Assert.NotNull(response);
            Assert.Equal("New Name", response.Name);
            Assert.Equal("newuser", response.Username);
            Assert.Equal("new@gmail.com", response.Email);
        }

        [Fact]
        public async Task DeleteUser_SetsDeletedAt()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var user = new User
            {
                UserId = 1,
                Name = "User",
                Username = "username",
                Email = "user@gmail.com",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new DeleteUserRequest { Id = 1 };
            var contextGrpc = TestServerCallContext.Create();

            var response = await service.DeleteUser(request, contextGrpc);

            Assert.NotNull(response);
            var deletedUser = await context.Users.FindAsync(1);
            Assert.NotNull(deletedUser?.DeletedAt);
        }

        [Fact]
        public async Task QueryUsers_ReturnsPagedFilteredResults()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Users.AddRange(
                new User { UserId = 1, Username = "firstuser", Name = "JohnTheFirst", Email = "john@gmail.com" },
                new User { UserId = 2, Username = "seconduser", Name = "MathewTheSecond", Email = "mathew@gmail.com" },
                new User { UserId = 3, Username = "third", Name = "MariusTheThird", Email = "marius@gmail.com" }
            );
            await context.SaveChangesAsync();

            var request = new QueryUsersRequest { QueryString = "second", Page = 1, PageSize = 10 };
            var grpc = TestServerCallContext.Create();

            var response = await service.QueryUsers(request, grpc);

            Assert.Single(response.Users);
            Assert.Equal("seconduser", response.Users[0].Username);
        }

        [Fact]
        public async Task QueryUsersForOrganization_ReturnsOnlyThatOrg()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Users.AddRange(
                new User { UserId = 1, Username = "user1", OrganizationId = 1 },
                new User { UserId = 2, Username = "user2", OrganizationId = 2 }
            );
            await context.SaveChangesAsync();

            var request = new QueryUsersForOrgRequest { OrganizationId = 1, Page = 1, PageSize = 10 };
            var grpc = TestServerCallContext.Create();

            var response = await service.QueryUsersForOrganization(request, grpc);

            Assert.Single(response.Users);
            Assert.Equal("user1", response.Users[0].Username);
        }

        [Fact]
        public async Task AssociateUserToOrganization_SetsOrganizationId()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Users.Add(new User { UserId = 1, Name = "Unassigned", OrganizationId = null });
            context.Organizations.Add(new Organization { OrganizationId = 100, Name = "Test Org" });
            await context.SaveChangesAsync();

            var request = new AssociationRequest { UserId = 1, OrganizationId = 100 };
            var contextGrpc = TestServerCallContext.Create();

            var response = await service.AssociateUserToOrganization(request, contextGrpc);

            var user = await context.Users.FindAsync(1);
            Assert.Equal(100, user!.OrganizationId);
        }

        [Fact]
        public async Task DisassociateUserFromOrganization_ClearsOrganizationId()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Users.Add(new User { UserId = 1, Name = "Assigned", OrganizationId = 100 });
            await context.SaveChangesAsync();

            var request = new DisassociationRequest { UserId = 1 };
            var contextGrpc = TestServerCallContext.Create();

            var response = await service.DisassociateUserFromOrganization(request, contextGrpc);

            var user = await context.Users.FindAsync(1);
            Assert.Null(user!.OrganizationId);
        }

        private IMapper CreateConfiguredMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            return config.CreateMapper();
        }
    }
}
