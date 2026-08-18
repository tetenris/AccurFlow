using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class UpdateDeliveryCommandHandler : IRequestHandler<UpdateDeliveryCommand>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDeliveryCommandHandler(
            IRepository<DeliveryOrderEntity> deliveryRepository,
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateDeliveryCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var delivery = await _deliveryRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == r.DeliveryOrderId && !x.IsDeleted, cancellationToken);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be edited");

            var order = await _orderRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.SalesOrderId == r.SalesOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");

            delivery.SalesOrderId = r.SalesOrderId;
            delivery.CustomerId = order.CustomerId;
            delivery.DeliveryDate = r.DeliveryDate;
            delivery.Notes = r.Notes;
            delivery.TotalAmount = 0;
            foreach (var existing in delivery.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
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
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = userId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}