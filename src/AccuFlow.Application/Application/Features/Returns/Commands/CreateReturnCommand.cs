using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Commands
{
    public record CreateReturnCommand(CreateReturnRequest Request, Guid UserId) : IRequest;
}