using System.Threading.Tasks;
using Grpc.Core;
using GrpcSample;
using Microsoft.AspNetCore.Authorization;

namespace GrpcService.Services
{
    [Authorize]
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<GreetReply> Greet(GreetRequest request, ServerCallContext context)
        {
            var identity = context.GetHttpContext().User.Identity;
            return Task.FromResult(new GreetReply
            {
                Message = $"Hello {request.Name}, IsAuthorized: {identity.IsAuthenticated}, Your account is {identity.Name}",
            });
        }

        [Authorize("Admins")]
        public override Task<GreetReply> GreetForAdmin(GreetRequest request, ServerCallContext context)
        {
            var identity = context.GetHttpContext().User.Identity;
            return Task.FromResult(new GreetReply
            {
                Message = $"Dear {request.Name}, IsAuthorized: {identity.IsAuthenticated}, Your account is {identity.Name}",
            });
        }
    }
}