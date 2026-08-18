using MediatR;

namespace AccuFlow.Application.Features.Returns.Commands
{
    public record DeleteReturnCommand(Guid GoodsReturnId, Guid UserId) : IRequest;
}