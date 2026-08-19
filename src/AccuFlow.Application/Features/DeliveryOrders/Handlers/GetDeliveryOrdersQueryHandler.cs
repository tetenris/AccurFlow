using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class GetDeliveryOrdersQueryHandler : IRequestHandler<GetDeliveryOrdersQuery, List<OrderOptionViewModel>>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IRepository<DeliveryOrderLineEntity> _deliveryLineRepository;

        public GetDeliveryOrdersQueryHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IRepository<DeliveryOrderLineEntity> deliveryLineRepository)
        {
            _orderRepository = orderRepository;
            _deliveryLineRepository = deliveryLineRepository;
        }

        public async Task<List<OrderOptionViewModel>> Handle(GetDeliveryOrdersQuery request, CancellationToken cancellationToken)
        {
            var deliveredByLine = await GetDeliveredBySoLineAsync(cancellationToken);
            var orders = await _orderRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.Status == "Approved" && !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new
                {
                    x.SalesOrderId,
                    x.OrderNumber,
                    x.OrderDate,
                    x.Customer.CustomerName,
                    Lines = x.Lines.Select(l => new { l.SalesOrderLineId, l.Quantity }).ToList()
                }).ToListAsync(cancellationToken);

            var result = new List<OrderOptionViewModel>();
            foreach (var o in orders)
            {
                var remaining = o.Lines.Sum(l =>
                {
                    var delivered = deliveredByLine.GetValueOrDefault(l.SalesOrderLineId, 0);
                    return Math.Max(0, l.Quantity - delivered);
                });
                if (remaining <= 0) continue;
                result.Add(new OrderOptionViewModel { SalesOrderId = o.SalesOrderId, OrderNumber = o.OrderNumber, CustomerName = o.CustomerName, OrderDate = o.OrderDate });
            }
            return result;
        }

        private async Task<Dictionary<Guid, decimal>> GetDeliveredBySoLineAsync(CancellationToken cancellationToken)
        {
            return await _deliveryLineRepository.Query()
                .Where(x => !x.IsDeleted && x.DeliveryOrder != null && !x.DeliveryOrder.IsDeleted && x.DeliveryOrder.Status == "Posted")
                .GroupBy(x => x.SalesOrderLineId)
                .Select(g => new { SalesOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.SalesOrderLineId, x => x.Quantity, cancellationToken);
        }
    }
}