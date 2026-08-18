using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Commands
{
    public record ApprovePurchaseRequestCommand(Guid Id, Guid UserId) : IRequest;
}