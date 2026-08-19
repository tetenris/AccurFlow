using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Commands
{
    public record UpdateTransferCommand(UpdateTransferRequest Request, Guid UserId) : IRequest;
}