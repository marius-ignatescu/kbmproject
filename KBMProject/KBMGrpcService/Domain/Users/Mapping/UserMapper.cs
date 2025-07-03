using KBMGrpcService.Models;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Domain.Users.Mapping
{
    /// <summary>
    /// Helper class used to map entities, requests and responses
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Creates a user response based on a User entity
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.UserId,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt.ToString(),
                UpdatedAt = user.UpdatedAt?.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Creates a User entity from a CreateUserRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static User CreateUserEntity(CreateUserRequest request)
        {
            return new User
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
