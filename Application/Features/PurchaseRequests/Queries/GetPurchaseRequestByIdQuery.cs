using AccuFlow.Models.PurchaseRequest;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Queries
{
    public record GetPurchaseRequestByIdQuery(Guid Id) : IRequest<PurchaseRequestDetailViewModel?>;
}