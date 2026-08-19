using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Commands
{
    public record UpdateReturnCommand(UpdateReturnRequest Request, Guid UserId) : IRequest;
}