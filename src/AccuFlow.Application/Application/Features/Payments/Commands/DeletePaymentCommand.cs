using MediatR;

namespace AccuFlow.Application.Features.Payments.Commands
{
    public record DeletePaymentCommand(Guid PaymentId, Guid UserId) : IRequest;
}