using Grpc.Core;
using KBMGrpcService;

namespace KBMGrpcService.Services
{
    public class UserService : UserProtoService.UserProtoServiceBase
    {
        private readonly ILogger<UserService> _logger;
        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        //public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        //{
        //}
    }
}
