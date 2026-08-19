using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.ProductionOrders.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ProductionOrders.Handlers
{
    public class PostProductionOrderCommandHandler : IRequestHandler<PostProductionOrderCommand>
    {
        private readonly IRepository<ProductionOrderEntity> _orderRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostProductionOrderCommandHandler(
            IRepository<ProductionOrderEntity> orderRepository,
            IRepository<ItemEntity> itemRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostProductionOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.ProductionOrderId == request.ProductionOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Production order not found");
            if (order.Status != "Draft") throw new Exception("Only draft production order can be posted");
            if (order.Lines.Count == 0) throw new Exception("Production order has no component lines");

            var onHand = await GetCurrentStockAsync(order.WarehouseId, cancellationToken);
            var finishedItem = await _itemRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemId == order.FinishedItemId && !x.IsDeleted, cancellationToken);
            if (finishedItem == null) throw new Exception("Finished item not found");

            foreach (var line in order.Lines)
            {
                var component = await _itemRepository.Query()
                    .FirstOrDefaultAsync(x => x.ItemId == line.ComponentItemId && !x.IsDeleted, cancellationToken);
                if (component == null) throw new Exception("Component item not found");
                var available = onHand.GetValueOrDefault(line.ComponentItemId, 0);
                if (line.QuantityRequired > available)
                    throw new Exception($"Insufficient stock for {component.ItemCode} ({component.ItemName}). Required: {line.QuantityRequired}, available: {available}");
                line.UnitCost = component.PurchasePrice;
            }

            var componentCostByAccount = new Dictionary<Guid, decimal>();
            foreach (var line in order.Lines)
            {
                if (line.UnitCost == 0 && line.QuantityRequired > 0)
                    throw new Exception($"Component cost required for quantity {line.QuantityRequired}. Set Purchase Price on the component item.");

                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = order.ProductionDate,
                    ItemId = line.ComponentItemId,
                    WarehouseId = order.WarehouseId,
                    MovementType = "Production Consumption",
                    SourceDocumentType = "ProductionOrder",
                    SourceDocumentId = order.ProductionOrderId,
                    QuantityIn = 0,
                    QuantityOut = line.QuantityRequired,
                    UnitCost = line.UnitCost,
                    Notes = $"Production {order.ProductionOrderNumber}",
                    CreatedBy = request.UserId.ToString()
                });

                var lineCost = line.QuantityRequired * line.UnitCost;
                var accountId = await GetItemInventoryAccountIdAsync(line.ComponentItemId, cancellationToken);
                if (!componentCostByAccount.ContainsKey(accountId)) componentCostByAccount[accountId] = 0;
                componentCostByAccount[accountId] += lineCost;
            }

            _stockMovementRepository.Add(new StockMovementEntity
            {
                StockMovementId = Guid.NewGuid(),
                MovementDate = order.ProductionDate,
                ItemId = order.FinishedItemId,
                WarehouseId = order.WarehouseId,
                MovementType = "Production Output",
                SourceDocumentType = "ProductionOrder",
                SourceDocumentId = order.ProductionOrderId,
                QuantityIn = order.Quantity,
                QuantityOut = 0,
                UnitCost = componentCostByAccount.Values.Sum(),
                Notes = $"Production {order.ProductionOrderNumber}",
                CreatedBy = request.UserId.ToString()
            });

            var journalId = await CreateProductionJournal(order, componentCostByAccount, request.UserId, cancellationToken);
            order.JournalId = journalId;
            order.Status = "Posted";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> CreateProductionJournal(ProductionOrderEntity order, Dictionary<Guid, decimal> componentCostByAccount, Guid userId, CancellationToken cancellationToken)
        {
            var description = $"Auto journal for production {order.ProductionOrderNumber}";
            var lines = new List<JournalLineRequest>();
            foreach (var entry in componentCostByAccount)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = entry.Key,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = entry.Value
                });
            }

            var finishedAccountId = await GetItemInventoryAccountIdAsync(order.FinishedItemId, cancellationToken);
            var totalCost = componentCostByAccount.Values.Sum();
            lines.Insert(0, new JournalLineRequest
            {
                AccountId = finishedAccountId,
                Description = description,
                DebitAmount = totalCost,
                CreditAmount = 0
            });

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = order.ProductionDate,
                Description = description,
                JournalLines = lines
            }, userId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = order.ProductionDate
            }, userId), cancellationToken);
            return journalId;
        }

        private async Task<Guid> GetItemInventoryAccountIdAsync(Guid itemId, CancellationToken cancellationToken)
        {
            var accountId = await _itemRepository.Query()
                .Where(x => x.ItemId == itemId && !x.IsDeleted)
                .Select(x => x.InventoryAccountId)
                .FirstOrDefaultAsync(cancellationToken);
            return accountId ?? Guid.Parse("10000000-0000-0000-0000-000000000005");
        }

        private async Task<Dictionary<Guid, decimal>> GetCurrentStockAsync(Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _stockMovementRepository.Query()
                .Where(x => x.WarehouseId == warehouseId && !x.IsDeleted)
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, Quantity = g.Sum(m => m.QuantityIn - m.QuantityOut) })
                .ToDictionaryAsync(x => x.ItemId, x => x.Quantity, cancellationToken);
        }
    }
}