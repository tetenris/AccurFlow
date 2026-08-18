using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Queries
{
    public record GetReturnInvoiceLinesQuery(Guid InvoiceId) : IRequest<ReturnInvoiceViewModel>;
}