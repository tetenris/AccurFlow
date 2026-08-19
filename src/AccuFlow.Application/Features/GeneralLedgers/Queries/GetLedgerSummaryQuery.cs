using AccuFlow.Models.GeneralLedger;
using MediatR;

namespace AccuFlow.Application.Features.GeneralLedgers.Queries
{
    public record GetLedgerSummaryQuery(GetLedgerSummaryRequest Request) : IRequest<List<LedgerSummaryViewModel>>;
}