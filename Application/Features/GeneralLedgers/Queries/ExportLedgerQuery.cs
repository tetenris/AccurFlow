using AccuFlow.Models.GeneralLedger;
using MediatR;

namespace AccuFlow.Application.Features.GeneralLedgers.Queries
{
    public record ExportLedgerQuery(GetLedgerRequest Request) : IRequest<byte[]>;
}