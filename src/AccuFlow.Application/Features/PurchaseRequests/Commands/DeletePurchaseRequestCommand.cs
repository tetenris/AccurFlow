using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Commands
{
    public record DeletePurchaseRequestCommand(Guid Id, Guid UserId) : IRequest;
}