using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Invoice;
using MediatR;

namespace AccuFlow.Application.Features.Invoices.Queries
{
    public record GetInvoiceDatatableQuery(DataTableInvoiceRequest Request) : IRequest<BaseDatatableResponse>;
}