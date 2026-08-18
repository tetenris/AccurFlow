using AccuFlow.Models.BaseModel;
using AccuFlow.Models.PurchaseRequest;
using MediatR;

namespace AccuFlow.Application.Features.PurchaseRequests.Queries
{
    public record GetPurchaseRequestDatatableQuery(DataTablePurchaseRequestRequest Request) : IRequest<BaseDatatableResponse>;
}