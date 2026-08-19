using AccuFlow.Models.Invoice;
using MediatR;

namespace AccuFlow.Application.Features.Invoices.Queries
{
    public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceDetailViewModel?>;
}