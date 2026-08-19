using MediatR;

namespace AccuFlow.Application.Features.Payments.Commands
{
    public record PostPaymentCommand(Guid PaymentId, Guid UserId) : IRequest;
}