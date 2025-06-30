using Grpc.Core;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Services
{
    public class UserService : UserProtoService.UserProtoServiceBase
    {
        private readonly ILogger<UserService> _logger;
        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            return new CreateUserResponse();
        }

        public override async Task<UserResponse> GetUserById(GetByIdRequest request, ServerCallContext context)
        {
            return new UserResponse();
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
