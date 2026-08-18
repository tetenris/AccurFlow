using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Commands
{
    public record UpdateReconciliationCommand(UpdateReconciliationRequest Request, Guid UserId) : IRequest;
}