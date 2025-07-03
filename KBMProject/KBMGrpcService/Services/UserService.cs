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
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username is required"));

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid email is required"));

            if (await _db.Users.AnyAsync(u => u.Username == request.Username && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));

            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.DeletedAt == null))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));

            var user = new User
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

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
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == request.Id && u.DeletedAt == null);

            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

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
        /// Returns a list of users by the specified search criteria
        /// </summary>
        /// <param name="request">The gRPC request containing search parameters</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="UserResponse"/> containing the the users list</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }
            var allowedColumns = new HashSet<string> { "Username", "Name", "Email", "CreatedAt" };

            if (!allowedColumns.Contains(request.OrderBy))
            {
                _logger.LogWarning("Invalid OrderBy column: {Column}. Defaulting to 'CreatedAt'.", request.OrderBy);
            }

            var query = _db.Users.AsNoTracking().Where(u => u.DeletedAt == null);

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.QueryString))
            {
                var q = request.QueryString.ToLower();
                query = query.Where(u =>
                    u.Name.Contains(q) ||
                    u.Username.Contains(q) ||
                    u.Email.Contains(q));
            }

            var total = await query.CountAsync();

            // Dynamic ordering with fallback
            if (!string.IsNullOrWhiteSpace(request.OrderBy))
            {
                var direction = string.Equals(request.Direction, "desc", StringComparison.OrdinalIgnoreCase)
                    ? "descending"
                    : "ascending";
                try
                {
                    query = query.OrderBy($"{request.OrderBy} {direction}");
                }
                catch
                {
                    query = query.OrderBy("CreatedAt");
                }
            }
            else
            {
                query = query.OrderBy(u => u.CreatedAt);
            }


            var sortColumn = string.IsNullOrWhiteSpace(request.OrderBy) ? "CreatedAt" : request.OrderBy;
            if (!allowedColumns.Contains(sortColumn))
            {
                // Fallback to CreatedAt sorting
                sortColumn = "CreatedAt";
            }

            // Paging
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Create response
            var response = new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };
            response.Users.AddRange(items.Select(u => new UserResponse
            {
                Id = u.UserId,
                Name = u.Name,
                Username = u.Username,
                Email = u.Email,
                CreatedAt = u.CreatedAt.ToString(),
                UpdatedAt = u.UpdatedAt?.ToString() ?? string.Empty
            }));

            return response;
        }

        public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            return new UserResponse();
        }

        public override async Task<DeleteResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            return new DeleteResponse();
        }
    }
}
