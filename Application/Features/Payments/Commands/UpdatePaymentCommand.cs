using AccuFlow.Models.Payment;
using MediatR;

namespace AccuFlow.Application.Features.Payments.Commands
{
    public record UpdatePaymentCommand(UpdatePaymentRequest Request, Guid UserId) : IRequest;
}