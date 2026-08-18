using AccuFlow.Models.Invoice;
using MediatR;

namespace AccuFlow.Application.Features.Invoices.Commands
{
    public record UpdateInvoiceCommand(UpdateInvoiceRequest Request, Guid UserId) : IRequest;
}