using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Queries
{
    public record GetBankStatementQuery(Guid AccountId, DateTime AsOfDate) : IRequest<List<ReconciliationLineViewModel>>;
}