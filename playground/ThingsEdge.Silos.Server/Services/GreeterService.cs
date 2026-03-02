using Grpc.Core;

namespace ThingsEdge.Silos.Server.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        logger.LogInformation("Sending hello to {Name}", request.Name);
        return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
    }
}
