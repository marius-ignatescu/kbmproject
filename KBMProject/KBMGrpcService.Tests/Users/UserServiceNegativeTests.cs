using AutoMapper;
using Grpc.Core;
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
    public class UserServiceNegativeTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetUserById_ThrowsRpcException_WhenUserNotFound()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);
            
            var request = new GetByIdRequest { Id = 123 };
            var grpc = TestServerCallContext.Create();

            var ex = await Assert.ThrowsAsync<RpcException>(() => service.GetUserById(request, grpc));
            Assert.Equal(StatusCode.NotFound, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_ThrowsRpcException_WhenUserNotFound()
        {
            using var context = GetInMemoryDbContext();
            var logger = new Mock<ILogger<UserService>>();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var request = new UpdateUserRequest { Id = 999, Name = "Doesn't Matter" };
            var grpc = TestServerCallContext.Create();

            var ex = await Assert.ThrowsAsync<RpcException>(() => service.UpdateUser(request, grpc));
            Assert.Equal(StatusCode.NotFound, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_ReturnFalse_WhenUserNotFound()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var request = new DeleteUserRequest { Id = 555 };
            var grpc = TestServerCallContext.Create();

            var response = await service.DeleteUser(request, grpc);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task AssociateUserToOrganization_ReturnsFalse_WhenUserNotFound()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Organizations.Add(new Organization { OrganizationId = 1, Name = "Valid Org" });
            await context.SaveChangesAsync();

            var request = new AssociationRequest { UserId = 99, OrganizationId = 1 };
            var grpc = TestServerCallContext.Create();

            var response = await service.AssociateUserToOrganization(request, grpc);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task AssociateUserToOrganization_ReturnsFalse_WhenOrganizationNotFound()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            context.Users.Add(new User { UserId = 1, Name = "User" });
            await context.SaveChangesAsync();

            var request = new AssociationRequest { UserId = 1, OrganizationId = 999 };
            var grpc = TestServerCallContext.Create();

            var response = await service.AssociateUserToOrganization(request, grpc);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task DisassociateUserFromOrganization_ReturnsFalse_WhenUserNotFound()
        {
            using var context = GetInMemoryDbContext();
            var userRepo = new UserRepository(context);
            var orgRepo = new OrganizationRepository(context);
            var logger = new Mock<ILogger<UserService>>();
            var mapper = CreateConfiguredMapper();
            var service = new UserService(userRepo, orgRepo, mapper, logger.Object);

            var request = new DisassociationRequest { UserId = 12345 };
            var grpc = TestServerCallContext.Create();

            var response = await service.DisassociateUserFromOrganization(request, grpc);
            Assert.False(response.Success);
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
