using AccuFlow.Models.ReceivablePayable;
using MediatR;

namespace AccuFlow.Application.Features.ReceivablePayables.Queries
{
    public record GetReceivablePayableQuery(ReceivablePayableRequest Request) : IRequest<List<ReceivablePayableViewModel>>;
}