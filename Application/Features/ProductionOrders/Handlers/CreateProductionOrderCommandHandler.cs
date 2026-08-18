using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ProductionOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ProductionOrders.Handlers
{
    public class CreateProductionOrderCommandHandler : IRequestHandler<CreateProductionOrderCommand>
    {
        private readonly IRepository<ProductionOrderEntity> _orderRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductionOrderCommandHandler(
            IRepository<ProductionOrderEntity> orderRepository,
            IRepository<ItemEntity> itemRepository,
            IRepository<BillOfMaterialEntity> bomRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _bomRepository = bomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateProductionOrderCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            if (r.FinishedItemId == Guid.Empty) throw new Exception("Finished item is required");
            if (r.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (r.Quantity <= 0) throw new Exception("Production quantity must be greater than zero");

            var item = await _itemRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemId == r.FinishedItemId && !x.IsDeleted, cancellationToken);
            if (item == null) throw new Exception("Finished item not found");

            var lines = new List<ProductionOrderLineEntity>();

            if (r.BomId.HasValue)
            {
                var bom = await _bomRepository.Query()
                    .Include(x => x.Lines.Where(l => !l.IsDeleted))
                    .FirstOrDefaultAsync(x => x.BomId == r.BomId.Value && !x.IsDeleted && x.IsActive, cancellationToken);
                if (bom == null) throw new Exception("Bill of material not found");
                if (bom.FinishedItemId != r.FinishedItemId) throw new Exception("BOM does not match the finished item");

                foreach (var line in bom.Lines)
                {
                    lines.Add(new ProductionOrderLineEntity
                    {
                        ProductionOrderLineId = Guid.NewGuid(),
                        ComponentItemId = line.ComponentItemId,
                        QuantityRequired = line.QuantityPerUnit * r.Quantity,
                        UnitCost = 0,
                        CreatedBy = userId.ToString()
                    });
                }
            }
            else
            {
                throw new Exception("Select a bill of material to produce from");
            }

            var order = new ProductionOrderEntity
            {
                ProductionOrderId = Guid.NewGuid(),
                ProductionOrderNumber = await GenerateProductionOrderNumberAsync(cancellationToken),
                BomId = r.BomId,
                FinishedItemId = r.FinishedItemId,
                Quantity = r.Quantity,
                ProductionDate = r.ProductionDate,
                WarehouseId = r.WarehouseId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            order.Lines = lines;

            _orderRepository.Add(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateProductionOrderNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _orderRepository.Query()
                .Where(x => x.ProductionOrderNumber.StartsWith("PRD-"))
                .OrderByDescending(x => x.ProductionOrderNumber)
                .Select(x => x.ProductionOrderNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"PRD-{next:D5}";
        }
    }
}