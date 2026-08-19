using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Commands;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class PostGoodsReceiptCommandHandler : IRequestHandler<PostGoodsReceiptCommand>
    {
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid DefaultInventoryAccountId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        private readonly IRepository<GoodsReceiptEntity> _grnRepository;
        private readonly IRepository<GoodsReceiptLineEntity> _grnLineRepository;
        private readonly IRepository<PurchaseOrderLineEntity> _poLineRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly ISender _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public PostGoodsReceiptCommandHandler(
            IRepository<GoodsReceiptEntity> grnRepository,
            IRepository<GoodsReceiptLineEntity> grnLineRepository,
            IRepository<PurchaseOrderLineEntity> poLineRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IRepository<ItemEntity> itemRepository,
            ISender mediator,
            IUnitOfWork unitOfWork)
        {
            _grnRepository = grnRepository;
            _grnLineRepository = grnLineRepository;
            _poLineRepository = poLineRepository;
            _stockMovementRepository = stockMovementRepository;
            _itemRepository = itemRepository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostGoodsReceiptCommand request, CancellationToken cancellationToken)
        {
            var grn = await _grnRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.GoodsReceiptId == request.Id && !x.IsDeleted, cancellationToken);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status == "Posted") throw new Exception("Goods receipt is already posted");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be posted");

            var receivedByLine = await GetReceivedByPoLineAsync(cancellationToken);
            foreach (var line in grn.Lines)
            {
                var poLine = await _poLineRepository.Query()
                    .FirstOrDefaultAsync(x => x.PurchaseOrderLineId == line.PurchaseOrderLineId && !x.IsDeleted, cancellationToken);
                if (poLine == null) throw new Exception($"Purchase order line not found for {line.Description}");
                var alreadyReceived = receivedByLine.GetValueOrDefault(line.PurchaseOrderLineId, 0);
                if (line.Quantity + alreadyReceived > poLine.Quantity)
                    throw new Exception($"Received quantity for {line.Description} exceeds remaining order quantity (remaining: {poLine.Quantity - alreadyReceived})");
            }

            foreach (var line in grn.Lines)
            {
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = grn.ReceiptDate,
                    ItemId = line.ItemId ?? Guid.Empty,
                    WarehouseId = grn.WarehouseId,
                    MovementType = "Goods Received",
                    SourceDocumentType = "GoodsReceipt",
                    SourceDocumentId = grn.GoodsReceiptId,
                    QuantityIn = line.Quantity,
                    QuantityOut = 0,
                    UnitCost = line.UnitPrice,
                    Notes = $"Goods receipt {grn.GoodsReceiptNumber}",
                    CreatedBy = request.UserId.ToString()
                });
            }

            var journalId = await CreateGoodsReceiptJournal(grn, request.UserId, cancellationToken);
            grn.JournalId = journalId;
            grn.Status = "Posted";
            grn.UpdatedAt = DateTime.UtcNow;
            grn.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> CreateGoodsReceiptJournal(GoodsReceiptEntity grn, Guid userId, CancellationToken cancellationToken)
        {
            var description = $"Auto journal for {grn.GoodsReceiptNumber}";
            var inventoryByAccount = new Dictionary<Guid, decimal>();

            foreach (var line in grn.Lines)
            {
                var accountId = await GetItemInventoryAccountIdAsync(line.ItemId, cancellationToken);
                var amount = line.Quantity * line.UnitPrice;
                if (!inventoryByAccount.ContainsKey(accountId)) inventoryByAccount[accountId] = 0;
                inventoryByAccount[accountId] += amount;
            }

            var journalLines = inventoryByAccount.Select(x => new JournalLineRequest
            {
                AccountId = x.Key,
                Description = description,
                DebitAmount = x.Value,
                CreditAmount = 0
            }).ToList();

            journalLines.Add(new JournalLineRequest
            {
                AccountId = AccountsPayableAccountId,
                Description = description,
                DebitAmount = 0,
                CreditAmount = grn.SubTotal
            });

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = grn.ReceiptDate,
                Description = description,
                JournalLines = journalLines
            }, userId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = grn.ReceiptDate
            }, userId), cancellationToken);

            return journalId;
        }

        private async Task<Guid> GetItemInventoryAccountIdAsync(Guid? itemId, CancellationToken cancellationToken)
        {
            if (!itemId.HasValue) return DefaultInventoryAccountId;
            var accountId = await _itemRepository.Query()
                .Where(x => x.ItemId == itemId.Value && !x.IsDeleted)
                .Select(x => x.InventoryAccountId)
                .FirstOrDefaultAsync(cancellationToken);
            return accountId ?? DefaultInventoryAccountId;
        }

        private async Task<Dictionary<Guid, decimal>> GetReceivedByPoLineAsync(CancellationToken cancellationToken)
        {
            return await _grnLineRepository.Query()
                .Where(x => !x.IsDeleted && x.GoodsReceipt != null && !x.GoodsReceipt.IsDeleted && x.GoodsReceipt.Status == "Posted")
                .GroupBy(x => x.PurchaseOrderLineId)
                .Select(g => new { PurchaseOrderLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.PurchaseOrderLineId, x => x.Quantity, cancellationToken);
        }
    }
}