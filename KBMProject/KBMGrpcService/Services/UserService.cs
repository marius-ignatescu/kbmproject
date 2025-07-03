using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Models;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

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

        public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
        {
            return new QueryUsersResponse();
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
