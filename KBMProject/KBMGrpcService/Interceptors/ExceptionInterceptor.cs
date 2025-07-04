using Grpc.Core.Interceptors;
using Grpc.Core;

namespace KBMGrpcService.Interceptors
{
    /// <summary>
    /// Intercept and log all unhandled exceptions in gRPC services
    /// </summary>
    public class ExceptionInterceptor : Interceptor
    {
        private readonly ILogger<ExceptionInterceptor> _logger;

        public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (RpcException rpcEx)
            {
                _logger.LogWarning(rpcEx, "Handled gRPC RpcException: {Message}", rpcEx.Status.Detail);
                throw;
            }
            catch (Exception ex)
            {
                // Log and convert to RpcException
                _logger.LogError(ex, "Unhandled exception caught in gRPC interceptor");
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred."));
            }
        }
    }
}
