using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Domain.Users.Validation;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Tests.Users
{
    public class UserValidatorTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task ValidateCreateUserRequest_Throws_WhenUsernameIsEmpty()
        {
            using var context = GetInMemoryDbContext();
            var request = new CreateUserRequest { Username = "", Email = "test@gmail.com" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                UserValidator.ValidateCreateUserRequest(request, context));

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        }

        [Fact]
        public async Task ValidateCreateUserRequest_Throws_WhenEmailIsInvalid()
        {
            using var context = GetInMemoryDbContext();
            var request = new CreateUserRequest { Username = "validuser", Email = "invalidemail" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                UserValidator.ValidateCreateUserRequest(request, context));

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        }

        [Fact]
        public async Task ValidateCreateUserRequest_Throws_WhenUsernameNotUnique()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Username = "existing", Email = "other@gmail.com", DeletedAt = null });
            await context.SaveChangesAsync();

            var request = new CreateUserRequest { Username = "existing", Email = "new@gmail.com" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                UserValidator.ValidateCreateUserRequest(request, context));

            Assert.Equal(StatusCode.AlreadyExists, ex.StatusCode);
        }

        [Fact]
        public async Task ValidateUserUpdateRequest_Throws_WhenEmailIsInvalid()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { UserId = 1, Username = "existing", Email = "old@gmail.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new UpdateUserRequest { Email = "invalid" };

            var ex = await Assert.ThrowsAsync<RpcException>(() =>
                UserValidator.ValidateUserUpdateRequest(request, user, context));

            Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        }
    }
}
