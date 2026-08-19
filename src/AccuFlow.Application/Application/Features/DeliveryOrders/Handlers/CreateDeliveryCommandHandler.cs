using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDeliveryCommandHandler(
            IRepository<DeliveryOrderEntity> deliveryRepository,
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.SalesOrderId == Guid.Empty) throw new Exception("Sales order is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var order = await _orderRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.SalesOrderId == r.SalesOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Approved") throw new Exception("Only approved sales order can be delivered");

            var doLineIds = order.Lines.Select(l => l.SalesOrderLineId).ToHashSet();
            if (r.Lines.Any(l => !doLineIds.Contains(l.SalesOrderLineId)))
                throw new Exception("Item line does not belong to the selected sales order");

            var delivery = new DeliveryOrderEntity
            {
                DeliveryOrderId = Guid.NewGuid(),
                DeliveryNumber = await GenerateNumberAsync(cancellationToken),
                DeliveryDate = r.DeliveryDate,
                SalesOrderId = r.SalesOrderId,
                CustomerId = order.CustomerId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = line.Quantity * line.UnitPrice;
                delivery.Lines.Add(new DeliveryOrderLineEntity
                {
                    DeliveryOrderLineId = Guid.NewGuid(),
                    SalesOrderLineId = line.SalesOrderLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                delivery.TotalAmount += lineTotal;
            }

            if (delivery.Lines.Count == 0) throw new Exception("At least one item line is required");
            _deliveryRepository.Add(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _deliveryRepository.Query()
                .Where(x => x.DeliveryNumber.StartsWith("DO-"))
                .OrderByDescending(x => x.DeliveryNumber)
                .Select(x => x.DeliveryNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"DO-{next:D5}";
        }
    }
}