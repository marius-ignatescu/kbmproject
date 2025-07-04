using System.Linq.Dynamic.Core;
using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Domain.Organizations;
using KBMGrpcService.Domain.Users;
using KBMGrpcService.Domain.Users.Extensions;
using KBMGrpcService.Domain.Users.Mapping;
using KBMGrpcService.Domain.Users.Queries;
using KBMGrpcService.Domain.Users.Validation;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

namespace KBMGrpcService.Services.Users
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

            await UserValidator.ValidateCreateUserRequest(request, _db);

            var user = UserMapper.CreateUserEntity(request);

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
        /// <param name="request">The gRPC request containing the required user id</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="UserResponse"/> containing the the user requested</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponse> GetUserById(GetByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", request.Id);

            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));
            }

            var user = await UserRepositoryHelper.GetActiveUserByIdWithNoTrackingAsync(_db, request.Id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            return UserMapper.MapToUserResponse(user);
        }

        /// <summary>
        /// Returns a list of users by the specified search criteria
        /// </summary>
        /// <param name="request">The gRPC request containing search parameters</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="QueryUsersResponse"/> containing the the users list</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
        {
            UserValidator.ValidatePagination(request);

            var query = UserRepositoryHelper.GetActiveUsers(_db);

            query = UserQueryBuilder.ApplyFiltering(query, request.QueryString);
            query = UserQueryBuilder.ApplyOrdering(query, request.OrderBy, request.Direction, _logger);

            var total = await query.CountAsync();

            var users = await UserQueryBuilder.ApplyPaging(query, request.Page, request.PageSize).ToListAsync();

            var response = new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };
            response.Users.AddRange(users.Select(UserMapper.MapToUserResponse));

            return response;
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="request">The gRPC request containing the user that will be updated</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="UserResponse"/> containing the result with the user updated operation</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            var user = await UserRepositoryHelper.GetActiveUserByIdAsync(_db, request.Id);
            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

            await UserValidator.ValidateUserUpdateRequest(request, user, _db);
            
            user.UpdateFromRequest(request);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", user.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update the user due to a database error."));
            }

            return UserMapper.MapToUserResponse(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="request">The gRPC request with the User ID that will be deleted</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="DeleteResponse"/> containing the result indicating if the operation was with success or not</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<DeleteResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            if (request.Id == 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID."));
            }

            var user = await UserRepositoryHelper.GetActiveUserByIdAsync(_db, request.Id);
            if (user == null)
            {
                return new DeleteResponse { Success = false };
            }

            bool success = false;
            try
            {
                success = await user.SoftDelete(_db);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", user.UserId);
            }

            return new DeleteResponse { Success = success };
        }

        /// <summary>
        /// Associates an user to ogranization
        /// </summary>
        /// <param name="request">The gRPC request</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="AssociationResponse"/> containing the result indicating if the operation was with success or not</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<AssociationResponse> AssociateUserToOrganization(AssociationRequest request, ServerCallContext context)
        {
            var user = await UserRepositoryHelper.GetActiveUserByIdAsync(_db, request.UserId);
            var org = await OrganizationRepositoryHelper.GetActiveOrganizationByIdAsync(_db, request.OrganizationId);

            if (user == null || org == null)
                return new AssociationResponse { Success = false };

            user.UpdateAssociationFromRequest(request);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to associate user {Username}", user.Username);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to associate the user."));
            }

            return new AssociationResponse { Success = true };
        }

        /// <summary>
        /// Dissassociates an user from ogranization
        /// </summary>
        /// <param name="request">The gRPC request</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="AssociationResponse"/> containing the result indicating if the operation was with success or not</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<AssociationResponse> DisassociateUserFromOrganization(DisassociationRequest request, ServerCallContext context)
        {
            var user = await UserRepositoryHelper.GetActiveUserByIdAsync(_db, request.UserId);
            
            if (user == null)
                return new AssociationResponse { Success = false };

            user.OrganizationId = null;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to dissassociate user {Username}", user.Username);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to dissassociate the user."));
            }

            return new AssociationResponse { Success = true };
        }

        /// <summary>
        /// Query users
        /// </summary>
        /// <param name="request">The gRPC request</param>
        /// <param name="context">The server call context for the current gRPC request</param>
        /// <returns>A <see cref="QueryUsersResponse"/> containing the result indicating if the operation was with success or not</returns>
        /// <exception cref="RpcException"></exception>
        public override async Task<QueryUsersResponse> QueryUsersForOrganization(QueryUsersForOrgRequest request, ServerCallContext context)
        {
            UserValidator.ValidatePagination(request);
            var query = UserRepositoryHelper.GetActiveUsersByOrganization(_db, request.OrganizationId);

            query = UserQueryBuilder.ApplyFiltering(query, request.QueryString);
            query = UserQueryBuilder.ApplyOrdering(query, request.OrderBy, request.Direction, _logger);

            var total = await query.CountAsync();

            var items = await UserQueryBuilder.ApplyPaging(query, request.Page, request.PageSize).ToListAsync();

            return new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Users = { items.Select(UserMapper.MapToUserResponse) }
            };
        }
    }
}
