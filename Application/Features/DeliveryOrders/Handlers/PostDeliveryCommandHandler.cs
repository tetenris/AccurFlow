using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class PostDeliveryCommandHandler : IRequestHandler<PostDeliveryCommand>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;
        private readonly IRepository<DeliveryOrderLineEntity> _deliveryLineRepository;
        private readonly IRepository<SalesOrderLineEntity> _soLineRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IRepository<WarehouseEntity> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostDeliveryCommandHandler(
            IRepository<DeliveryOrderEntity> deliveryRepository,
            IRepository<DeliveryOrderLineEntity> deliveryLineRepository,
            IRepository<SalesOrderLineEntity> soLineRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IRepository<WarehouseEntity> warehouseRepository,
            IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            _deliveryLineRepository = deliveryLineRepository;
            _soLineRepository = soLineRepository;
            _stockMovementRepository = stockMovementRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status == "Posted") throw new Exception("Delivery order is already posted");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be posted");

            var deliveredByLine = await _deliveryLineRepository.Query()
                .Where(x => !x.IsDeleted && x.DeliveryOrder != null && !x.DeliveryOrder.IsDeleted && x.DeliveryOrder.Status == "Posted")
                .GroupBy(x => x.SalesOrderLineId)
                .Select(g => new { SalesOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.SalesOrderLineId, x => x.Quantity, cancellationToken);

            foreach (var line in delivery.Lines)
            {
                var soLine = await _soLineRepository.Query()
                    .FirstOrDefaultAsync(x => x.SalesOrderLineId == line.SalesOrderLineId && !x.IsDeleted, cancellationToken);
                if (soLine == null) throw new Exception($"Sales order line not found for {line.Description}");
                var alreadyDelivered = deliveredByLine.GetValueOrDefault(line.SalesOrderLineId, 0);
                if (line.Quantity + alreadyDelivered > soLine.Quantity)
                    throw new Exception($"Delivered quantity for {line.Description} exceeds order quantity (remaining: {soLine.Quantity - alreadyDelivered})");
            }

            var defaultWarehouseId = await _warehouseRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.WarehouseCode)
                .Select(x => x.WarehouseId)
                .FirstOrDefaultAsync(cancellationToken);

            foreach (var line in delivery.Lines)
            {
                if (!line.ItemId.HasValue) continue;
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = delivery.DeliveryDate,
                    ItemId = line.ItemId.Value,
                    WarehouseId = defaultWarehouseId,
                    MovementType = "Sales Delivery",
                    SourceDocumentType = "DeliveryOrder",
                    SourceDocumentId = delivery.DeliveryOrderId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = line.UnitPrice,
                    Notes = $"Delivery {delivery.DeliveryNumber}",
                    CreatedBy = request.UserId.ToString()
                });
            }

            delivery.Status = "Posted";
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}