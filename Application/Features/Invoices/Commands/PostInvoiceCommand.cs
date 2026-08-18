using MediatR;

namespace AccuFlow.Application.Features.Invoices.Commands
{
    public record PostInvoiceCommand(Guid InvoiceId, Guid UserId) : IRequest;
}