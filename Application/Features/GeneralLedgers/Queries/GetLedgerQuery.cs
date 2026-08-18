using AccuFlow.Models.GeneralLedger;
using MediatR;

namespace AccuFlow.Application.Features.GeneralLedgers.Queries
{
    public record GetLedgerQuery(GetLedgerRequest Request) : IRequest<AccountLedgerViewModel>;
}