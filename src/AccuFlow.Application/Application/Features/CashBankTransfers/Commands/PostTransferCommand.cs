using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Commands
{
    public record PostTransferCommand(Guid TransferId, Guid UserId) : IRequest;
}