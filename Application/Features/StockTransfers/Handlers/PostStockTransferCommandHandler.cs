using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class PostStockTransferCommandHandler : IRequestHandler<PostStockTransferCommand>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostStockTransferCommandHandler(
            IRepository<StockTransferEntity> stockTransferRepository,
            IRepository<ItemEntity> itemRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IUnitOfWork unitOfWork)
        {
            _stockTransferRepository = stockTransferRepository;
            _itemRepository = itemRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostStockTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _stockTransferRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.StockTransferId == request.Id && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Stock transfer not found");
            if (transfer.Status == "Posted") throw new Exception("Stock transfer is already posted");
            if (transfer.Status != "Draft") throw new Exception("Only draft stock transfer can be posted");

            foreach (var line in transfer.Lines)
            {
                var available = await GetAvailableStockAsync(line.ItemId, transfer.FromWarehouseId, cancellationToken);
                if (available < line.Quantity)
                    throw new Exception($"Insufficient stock for {await GetItemLabelAsync(line.ItemId, cancellationToken)} in source warehouse (available: {available}, transfer: {line.Quantity})");
                var unitCost = await _itemRepository.Query()
                    .Where(x => x.ItemId == line.ItemId)
                    .Select(x => x.PurchasePrice)
                    .FirstOrDefaultAsync(cancellationToken);
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = transfer.TransferDate,
                    ItemId = line.ItemId,
                    WarehouseId = transfer.FromWarehouseId,
                    MovementType = "Stock Transfer Out",
                    SourceDocumentType = "StockTransfer",
                    SourceDocumentId = transfer.StockTransferId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = unitCost,
                    Notes = $"Transfer {transfer.StockTransferNumber} to warehouse",
                    CreatedBy = request.UserId.ToString()
                });
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = transfer.TransferDate,
                    ItemId = line.ItemId,
                    WarehouseId = transfer.ToWarehouseId,
                    MovementType = "Stock Transfer In",
                    SourceDocumentType = "StockTransfer",
                    SourceDocumentId = transfer.StockTransferId,
                    QuantityIn = line.Quantity,
                    QuantityOut = 0,
                    UnitCost = unitCost,
                    Notes = $"Transfer {transfer.StockTransferNumber} from warehouse",
                    CreatedBy = request.UserId.ToString()
                });
            }

            transfer.Status = "Posted";
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<decimal> GetAvailableStockAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken)
        {
            return await _stockMovementRepository.Query()
                .Where(x => x.ItemId == itemId && x.WarehouseId == warehouseId && !x.IsDeleted)
                .SumAsync(x => x.QuantityIn - x.QuantityOut, cancellationToken);
        }

        private async Task<string> GetItemLabelAsync(Guid itemId, CancellationToken cancellationToken)
        {
            var item = await _itemRepository.Query()
                .Where(x => x.ItemId == itemId)
                .Select(x => new { x.ItemCode, x.ItemName })
                .FirstOrDefaultAsync(cancellationToken);
            return item == null ? itemId.ToString() : $"{item.ItemCode} - {item.ItemName}";
        }
    }
}