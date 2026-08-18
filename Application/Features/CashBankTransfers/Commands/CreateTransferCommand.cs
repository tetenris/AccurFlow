using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.CashBankTransfers.Commands
{
    public record CreateTransferCommand(CreateTransferRequest Request, Guid UserId) : IRequest;
}