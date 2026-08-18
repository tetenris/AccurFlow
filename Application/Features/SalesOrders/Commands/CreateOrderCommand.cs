using AccuFlow.Models.SalesOrder;
using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Commands
{
    public record CreateOrderCommand(CreateOrderRequest Request, Guid UserId) : IRequest;
}