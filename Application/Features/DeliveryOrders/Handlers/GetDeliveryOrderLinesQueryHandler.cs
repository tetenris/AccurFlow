using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class GetDeliveryOrderLinesQueryHandler : IRequestHandler<GetDeliveryOrderLinesQuery, OrderDeliveryViewModel>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IRepository<DeliveryOrderLineEntity> _deliveryLineRepository;

        public GetDeliveryOrderLinesQueryHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IRepository<DeliveryOrderLineEntity> deliveryLineRepository)
        {
            _orderRepository = orderRepository;
            _deliveryLineRepository = deliveryLineRepository;
        }

        public async Task<OrderDeliveryViewModel> Handle(GetDeliveryOrderLinesQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.SalesOrderId == request.SalesOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");

            var deliveredByLine = await _deliveryLineRepository.Query()
                .Where(x => !x.IsDeleted && x.DeliveryOrder != null && !x.DeliveryOrder.IsDeleted && x.DeliveryOrder.Status == "Posted")
                .GroupBy(x => x.SalesOrderLineId)
                .Select(g => new { SalesOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.SalesOrderLineId, x => x.Quantity, cancellationToken);

            var lines = order.Lines.Where(l => !l.IsDeleted).Select(l =>
            {
                var delivered = deliveredByLine.GetValueOrDefault(l.SalesOrderLineId, 0);
                return new OrderDeliveryLineViewModel
                {
                    SalesOrderLineId = l.SalesOrderLineId,
                    ItemId = l.ItemId,
                    ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                    ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    DeliveredQuantity = delivered,
                    RemainingQuantity = l.Quantity - delivered,
                    UnitPrice = l.UnitPrice
                };
            }).Where(x => x.RemainingQuantity > 0).ToList();

            return new OrderDeliveryViewModel
            {
                SalesOrderId = order.SalesOrderId,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.CustomerName,
                OrderDate = order.OrderDate,
                Lines = lines
            };
        }
    }
}