using AccuFlow.Models.Invoice;
using MediatR;

namespace AccuFlow.Application.Features.Invoices.Commands
{
    public record CreateInvoiceCommand(CreateInvoiceRequest Request, Guid UserId) : IRequest;
}