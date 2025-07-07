using KBMGrpcService.Domain.Users.Mapping;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Tests.Users
{
    public class UserMapperTests
    {
        [Fact]
        public void MapToUserResponse_MapsCorrectly()
        {
            var entity = new User
            {
                UserId = 1,
                Name = "Test User",
                Username = "testuser",
                Email = "test@gmail.com",
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 7, 1)
            };

            var response = UserMapper.MapToUserResponse(entity);

            Assert.Equal("Test User", response.Name);
            Assert.Equal("testuser", response.Username);
            Assert.Equal("test@gmail.com", response.Email);
            Assert.Equal("1", response.Id.ToString());
            Assert.Equal("1/1/2025 12:00:00 AM", response.CreatedAt);
            Assert.Equal("7/1/2025 12:00:00 AM", response.UpdatedAt);
        }

        [Fact]
        public void MapToUserResponse_HandlesNulls()
        {
            var entity = new User
            {
                UserId = 2,
                Name = "Null User",
                Username = "nulluser",
                Email = "null@gmail.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var response = UserMapper.MapToUserResponse(entity);

            Assert.Equal(string.Empty, response.UpdatedAt);
        }

        [Fact]
        public void CreateUserEntity_MapsCorrectly()
        {
            var request = new CreateUserRequest
            {
                Name = "New User",
                Username = "newuser",
                Email = "new@gmail.com"
            };

            var entity = UserMapper.CreateUserEntity(request);

            Assert.Equal("New User", entity.Name);
            Assert.Equal("newuser", entity.Username);
            Assert.Equal("new@gmail.com", entity.Email);
            Assert.True((DateTime.UtcNow - entity.CreatedAt).TotalSeconds < 5);
        }
    }
}
