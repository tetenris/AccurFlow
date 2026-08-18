using AccuFlow.Models.BaseModel;
using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Queries
{
    public record GetTransferDatatableQuery(DataTableTransferRequest Request) : IRequest<BaseDatatableResponse>;
}