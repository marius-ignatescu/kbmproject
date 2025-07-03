using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace KBMGrpcService.Services
{
    /// <summary>
    /// gRPC service responsable for maganing user-related operations
    /// </summary>
    public class UserService : UserProtoService.UserProtoServiceBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="db">The database context</param>
        /// <param name="logger">The logger used to record service level events</param>
        public UserService(AppDbContext db, ILogger<UserService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Handles the creation of a user
        /// </summary>
        /// <param name="request">The gRPC request containing the user that will be created</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="CreateUserResponse"/> containing the result of the user creation operation</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            await ValidateCreateUserRequest(request);

            var user = CreateUserEntity(request);

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create user {Username}", user.Username);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create the user."));
            }

            return new CreateUserResponse { Id = user.UserId };
        }

        /// <summary>
        /// Returns a user by id
        /// </summary>
        /// <param name="request">The gRPC request containing the user that will be created</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="UserResponse"/> containing the the user requested</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponse> GetUserById(GetByIdRequest request, ServerCallContext context)
        {
            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));
            }

            var user = await GetActiveUserByIdAsync(request.Id);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            return MapToUserResponse(user);
        }

        /// <summary>
        /// Returns a list of users by the specified search criteria
        /// </summary>
        /// <param name="request">The gRPC request containing search parameters</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="UserResponse"/> containing the the users list</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
        {
            ValidatePagination(request);

            var query = _db.Users.AsNoTracking().Where(u => u.DeletedAt == null);

            query = ApplyFiltering(query, request.QueryString);
            query = ApplyOrdering(query, request.OrderBy, request.Direction, _logger);

            var total = await query.CountAsync();

            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var response = new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };
            response.Users.AddRange(users.Select(MapToUserResponse));

            return response;
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="request">The gRPC request containing the user that will be updated</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="CreateUserResponse"/> containing the result with the user updated operation</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            request.Email = request.Email?.Trim();
            request.Username = request.Username?.Trim();
            request.Name = request.Name?.Trim();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == request.Id && u.DeletedAt == null);
            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

            await ValidateUserUpdateRequest(request, user);
            UpdateUserFields(user, request);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", user.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update the user due to a database error."));
            }

            return MapToUserResponse(user);
        }

        public override async Task<DeleteResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            return new DeleteResponse();
        }

        private async Task ValidateCreateUserRequest(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username is required"));

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid email is required"));

            if (await _db.Users.AnyAsync(u => u.Username == request.Username && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));

            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));
        }

        private async Task ValidateUserUpdateRequest(UpdateUserRequest request, User existingUser)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "A valid email address is required."));

            if (!string.IsNullOrEmpty(request.Username))
            {
                var usernameExists = await _db.Users.AnyAsync(u =>
                    u.Username == request.Username && u.UserId != existingUser.UserId && u.DeletedAt == null);

                if (usernameExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await _db.Users.AnyAsync(u =>
                    u.Email == request.Email && u.UserId != existingUser.UserId && u.DeletedAt == null);

                if (emailExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));
            }
        }

        private static void ValidatePagination(QueryUsersRequest request)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }
        }

        private async Task<User?> GetActiveUserByIdAsync(int id)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id && u.DeletedAt == null);
        }

        private static void UpdateUserFields(User user, UpdateUserRequest request)
        {
            user.Name = request.Name ?? user.Name;
            user.Username = request.Username ?? user.Username;
            user.Email = request.Email ?? user.Email;
            user.UpdatedAt = DateTime.UtcNow;
        }

        private static User CreateUserEntity(CreateUserRequest request)
        {
            return new User
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static readonly HashSet<string> AllowedOrderColumns = new() { "Username", "Name", "Email", "CreatedAt" };

        private IQueryable<User> ApplyOrdering(IQueryable<User> query, string? orderBy, string? direction, ILogger logger)
        {
            var column = string.IsNullOrWhiteSpace(orderBy) ? "CreatedAt" : orderBy;
            var orderDir = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";

            if (!AllowedOrderColumns.Contains(column))
            {
                logger.LogWarning("Invalid OrderBy column: {Column}. Defaulting to 'CreatedAt'.", column);
                column = "CreatedAt";
            }

            try
            {
                return query.OrderBy($"{column} {orderDir}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply ordering. Defaulting to 'CreatedAt'.");
                return query.OrderBy("CreatedAt");
            }
        }

        private static IQueryable<User> ApplyFiltering(IQueryable<User> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var q = search.ToLower();
            return query.Where(u =>
                u.Name.ToLower().Contains(q) ||
                u.Username.ToLower().Contains(q) ||
                u.Email.ToLower().Contains(q));
        }

        private static UserResponse MapToUserResponse(User user)
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
    }
}
