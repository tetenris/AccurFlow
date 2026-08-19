using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payment;
using MediatR;

namespace AccuFlow.Application.Features.Payments.Queries
{
    public record GetPaymentDatatableQuery(DataTablePaymentRequest Request) : IRequest<BaseDatatableResponse>;
}