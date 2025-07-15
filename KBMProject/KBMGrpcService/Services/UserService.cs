using System.Linq.Dynamic.Core;
using AutoMapper;
using Grpc.Core;
using KBMGrpcService.Data;
using KBMGrpcService.Data.QueryBuilders;
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
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record service level events</param>
        public UserService(IUserRepository userRepository, IOrganizationRepository organizationRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _mapper = mapper;
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
            // Validation
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username is required"));

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Valid email is required"));

            if (await _userRepository.UsernameExistsAsync(request.Username))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));

            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));

            // Create and save the user
            var user = _mapper.Map<User>(request);

            try
            {
                await _userRepository.AddAsync(user);
                _userRepository.SaveChanges();
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

            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            return _mapper.Map<UserResponse>(user);
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
            // Validation
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }

            var query = await _userRepository.GetAllAsync();

            query = UserQueryBuilder.ApplyFiltering(query, request.QueryString);
            query = UserQueryBuilder.ApplyOrdering(query.AsQueryable(), request.OrderBy, request.Direction, _logger);

            var total = query.Count();

            var users = UserQueryBuilder.ApplyPaging(query, request.Page, request.PageSize).ToList();

            var response = new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };

            response.Users.AddRange(_mapper.Map<IEnumerable<UserResponse>>(users));

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
            // Validations
            if (request == null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));

            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "A valid email address is required."));

            if (!string.IsNullOrEmpty(request.Username))
            {
                var usernameExists = await _userRepository.UserExistsByUsernameAsync(request.Username, user.UserId);

                if (usernameExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Username must be unique"));
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailExists = await _userRepository.UserExistsByEmailAsync(request.Email, user.UserId);

                if (emailExists)
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "Email must be unique"));
            }

            // Update
            try
            {
                // Map request
                _mapper.Map(request, user);

                _userRepository.Update(user);
                _userRepository.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", user.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update the user due to a database error."));
            }

            // Return the response
            return _mapper.Map<UserResponse>(user);
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

            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return new DeleteResponse { Success = false };
            }

            bool success = false;
            try
            {
                _userRepository.Delete(user);
                success = _organizationRepository.SaveChanges();
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
            var user = await _userRepository.GetByIdAsync(request.UserId);
            var org = await _organizationRepository.GetByIdAsync(request.OrganizationId);

            if (user == null || org == null)
                return new AssociationResponse { Success = false };

            bool succes = false;
            try
            {
                user.OrganizationId = request.OrganizationId;
                _userRepository.Update(user);
                succes = _userRepository.SaveChanges();
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
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                return new AssociationResponse { Success = false };

            bool succes = false;
            try
            {
                user.OrganizationId = null;
                _userRepository.Update(user);
                succes = _userRepository.SaveChanges();
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
            // Validations
            if (request.Page < 1 || request.PageSize < 1)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Page and PageSize must be greater than 0."));
            }

            var query = await _userRepository.GetUsersByOrganization(request.OrganizationId);

            query = UserQueryBuilder.ApplyFiltering(query, request.QueryString);
            query = UserQueryBuilder.ApplyOrdering(query.AsQueryable(), request.OrderBy, request.Direction, _logger);

            var total = query.Count();

            var users = UserQueryBuilder.ApplyPaging(query, request.Page, request.PageSize).ToList();

            // Build response
            var response = new QueryUsersResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total
            };

            response.Users.AddRange(_mapper.Map<IEnumerable<UserResponse>>(users));

            return response;
        }
    }
}
