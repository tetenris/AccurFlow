using AccuFlow.Models.SalesOrder;
using MediatR;

namespace AccuFlow.Application.Features.SalesOrders.Commands
{
    public record UpdateOrderCommand(UpdateOrderRequest Request, Guid UserId) : IRequest;
}