using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Commands
{
    public record CreateReconciliationCommand(CreateReconciliationRequest Request, Guid UserId) : IRequest;
}