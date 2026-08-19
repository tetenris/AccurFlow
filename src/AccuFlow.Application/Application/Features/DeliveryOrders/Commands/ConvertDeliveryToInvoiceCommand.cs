using MediatR;

namespace AccuFlow.Application.Features.DeliveryOrders.Commands
{
    public record ConvertDeliveryToInvoiceCommand(Guid Id, Guid UserId) : IRequest;
}