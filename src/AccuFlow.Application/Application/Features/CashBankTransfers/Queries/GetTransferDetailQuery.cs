using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Queries
{
    public record GetTransferDetailQuery(Guid TransferId) : IRequest<TransferDetailViewModel?>;
}