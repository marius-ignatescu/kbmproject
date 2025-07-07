using Grpc.Core;

namespace KBMGrpcService.Tests
{
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
