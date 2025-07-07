using Grpc.Core;
using KBMGrpcService.Interceptors;
using Microsoft.Extensions.Logging;
using Moq;

namespace KBMGrpcService.Tests.Interceptors
{
    public class ExceptionInterceptorTests
    {
        private readonly Mock<ILogger<ExceptionInterceptor>> _loggerMock = new();

        private ExceptionInterceptor CreateInterceptor() =>
            new ExceptionInterceptor(_loggerMock.Object);

        [Fact]
        public async Task UnaryServerHandler_AllowsNormalExecution()
        {
            var interceptor = CreateInterceptor();
            var context = TestServerCallContext.Create();

            var result = await interceptor.UnaryServerHandler<string, string>(
                "request",
                context,
                (req, ctx) => Task.FromResult("success"));

            Assert.Equal("success", result);
        }

        [Fact]
        public async Task UnaryServerHandler_HandlesRpcExceptionAndLogsWarning()
        {
            var interceptor = CreateInterceptor();
            var context = TestServerCallContext.Create();

            var ex = new RpcException(new Status(StatusCode.InvalidArgument, "Invalid input"));

            var caught = await Assert.ThrowsAsync<RpcException>(() =>
                interceptor.UnaryServerHandler<string, string>("req", context, (req, ctx) => throw ex));

            Assert.Equal(StatusCode.InvalidArgument, caught.StatusCode);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Handled gRPC RpcException")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UnaryServerHandler_HandlesExceptionAndReturnsRpcInternal()
        {
            var interceptor = CreateInterceptor();
            var context = TestServerCallContext.Create();

            var ex = new InvalidOperationException("unexpected");

            var caught = await Assert.ThrowsAsync<RpcException>(() =>
                interceptor.UnaryServerHandler<string, string>("req", context, (req, ctx) => throw ex));

            Assert.Equal(StatusCode.Internal, caught.StatusCode);
            Assert.Equal("An internal error occurred.", caught.Status.Detail);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Unhandled exception")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal TestServerCallContext
    public class TestServerCallContext : ServerCallContext
    {
        public static ServerCallContext Create() =>
            TestServerCallContextHelper.Create();

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null!;
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
        protected override string MethodCore => "TestMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "localhost";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new();
        protected override CancellationToken CancellationTokenCore => default;
        protected override Metadata ResponseTrailersCore => new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(null!, new());
    }

    public static class TestServerCallContextHelper
    {
        public static ServerCallContext Create() => new TestServerCallContext();
    }
}
