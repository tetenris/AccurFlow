using MediatR;

namespace AccuFlow.Application.Features.Invoices.Commands
{
    public record DeleteInvoiceCommand(Guid InvoiceId, Guid UserId) : IRequest;
}