using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Domain.Users.Validation
{
    /// <summary>
    /// Helper class used to validate User entities
    /// </summary>
    public static class UserValidator
    {
        public static async Task ValidateCreateUserRequest(CreateUserRequest request, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username is required"));

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid email is required"));

            if (await db.Users.AnyAsync(u => u.Username == request.Username && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));

            if (await db.Users.AnyAsync(u => u.Email == request.Email && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));
        }

        public static async Task ValidateUserUpdateRequest(UpdateUserRequest request, User existingUser, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "A valid email address is required."));

            if (!string.IsNullOrEmpty(request.Username))
            {
                var usernameExists = await UserRepositoryHelper.UserExistsByUsernameAsync(db, request.Username, existingUser.UserId);

                if (usernameExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await UserRepositoryHelper.UserExistsByEmailAsync(db, request.Email, existingUser.UserId);

                if (emailExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));
            }
        }

        public static void ValidatePagination(QueryUsersRequest request)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }
        }

        public static void ValidatePagination(QueryUsersForOrgRequest request)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }
        }
    }
}
