using AccuFlow.Models.PurchaseRequest;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Commands
{
    public record CreatePurchaseRequestCommand(CreatePurchaseRequestRequest Request, Guid UserId) : IRequest;
}