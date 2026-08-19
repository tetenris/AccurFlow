using AccuFlow.Models.Payment;
using MediatR;

namespace AccuFlow.Application.Features.Payments.Queries
{
    public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDetailViewModel?>;
}