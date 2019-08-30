using System.Threading.Tasks;
using Grpc.Core;
using GrpcSample;

namespace GrpcService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<GreetReply> Greet(GreetRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GreetReply
            {
                Message = $"Hello {request.Name}",
            });
        }
    }
}