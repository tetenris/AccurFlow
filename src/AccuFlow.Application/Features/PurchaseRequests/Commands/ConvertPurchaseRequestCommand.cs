using AccuFlow.Models.PurchaseRequest;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Commands
{
    public record ConvertPurchaseRequestCommand(ConvertPurchaseRequestRequest Request, Guid UserId) : IRequest<Guid>;
}