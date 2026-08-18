using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Commands
{
    public record DeleteTransferCommand(Guid TransferId, Guid UserId) : IRequest;
}