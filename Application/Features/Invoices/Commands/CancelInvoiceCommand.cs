using MediatR;

namespace AccuFlow.Application.Features.Invoices.Commands
{
    public record CancelInvoiceCommand(Guid InvoiceId, Guid UserId) : IRequest;
}