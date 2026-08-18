using AccuFlow.Models.Payment;
using MediatR;

namespace AccuFlow.Application.Features.Payments.Commands
{
    public record CreatePaymentCommand(CreatePaymentRequest Request, Guid UserId) : IRequest;
}