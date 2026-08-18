using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Queries
{
    public record GetReconciliationDetailQuery(Guid ReconciliationId) : IRequest<ReconciliationDetailViewModel?>;
}