using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.Returns.Commands;
using AccuFlow.Application.Features.Returns.Helpers;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class PostReturnCommandHandler : IRequestHandler<PostReturnCommand>
    {
        private static readonly Guid SalesRevenueAccountId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        private static readonly Guid TaxPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid DefaultInventoryAccountId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;
        private readonly IRepository<GoodsReturnLineEntity> _goodsReturnLineRepository;
        private readonly IRepository<InvoiceLineEntity> _invoiceLineRepository;
        private readonly IRepository<StockMovementEntity> _stockMovementRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostReturnCommandHandler(
            IRepository<GoodsReturnEntity> goodsReturnRepository,
            IRepository<GoodsReturnLineEntity> goodsReturnLineRepository,
            IRepository<InvoiceLineEntity> invoiceLineRepository,
            IRepository<StockMovementEntity> stockMovementRepository,
            IRepository<ItemEntity> itemRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _goodsReturnRepository = goodsReturnRepository;
            _goodsReturnLineRepository = goodsReturnLineRepository;
            _invoiceLineRepository = invoiceLineRepository;
            _stockMovementRepository = stockMovementRepository;
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostReturnCommand request, CancellationToken cancellationToken)
        {
            var returnDoc = await _goodsReturnRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.GoodsReturnId == request.GoodsReturnId && !x.IsDeleted, cancellationToken);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status == "Posted") throw new Exception("Return is already posted");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be posted");

            var returnedByLine = await ReturnHelper.GetReturnedByInvoiceLineAsync(_goodsReturnLineRepository, cancellationToken);
            foreach (var line in returnDoc.Lines)
            {
                var invoiceLine = await _invoiceLineRepository.FirstOrDefaultAsync(
                    x => x.InvoiceLineId == line.InvoiceLineId && !x.IsDeleted, cancellationToken);
                if (invoiceLine == null) throw new Exception($"Invoice line not found for {line.Description}");
                var alreadyReturned = returnedByLine.GetValueOrDefault(line.InvoiceLineId, 0);
                if (line.Quantity + alreadyReturned > invoiceLine.Quantity)
                    throw new Exception($"Returned quantity for {line.Description} exceeds invoice quantity (remaining: {invoiceLine.Quantity - alreadyReturned})");
            }

            foreach (var line in returnDoc.Lines)
            {
                if (!line.ItemId.HasValue) continue;
                _stockMovementRepository.Add(new StockMovementEntity
                {
                    StockMovementId = Guid.NewGuid(),
                    MovementDate = returnDoc.ReturnDate,
                    ItemId = line.ItemId.Value,
                    WarehouseId = returnDoc.WarehouseId,
                    MovementType = returnDoc.ReturnType == "Sales" ? "Sales Return" : "Purchase Return",
                    SourceDocumentType = "GoodsReturn",
                    SourceDocumentId = returnDoc.GoodsReturnId,
                    QuantityIn = 0,
                    QuantityOut = line.Quantity,
                    UnitCost = line.UnitPrice,
                    Notes = $"Return {returnDoc.GoodsReturnNumber}",
                    CreatedBy = request.UserId.ToString()
                });
            }

            var journalId = await CreateReturnJournal(returnDoc, request.UserId, cancellationToken);
            returnDoc.JournalId = journalId;
            returnDoc.Status = "Posted";
            returnDoc.UpdatedAt = DateTime.UtcNow;
            returnDoc.UpdatedBy = request.UserId.ToString();

            _goodsReturnRepository.Update(returnDoc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> CreateReturnJournal(GoodsReturnEntity returnDoc, Guid userId, CancellationToken cancellationToken)
        {
            var description = $"Auto journal for {returnDoc.GoodsReturnNumber}";
            var journalLines = new List<JournalLineRequest>();

            if (returnDoc.ReturnType == "Sales")
            {
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = SalesRevenueAccountId,
                    Description = description,
                    DebitAmount = returnDoc.SubTotal,
                    CreditAmount = 0
                });
                if (returnDoc.TaxAmount > 0)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = returnDoc.TaxAmount,
                        CreditAmount = 0
                    });
                }
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = returnDoc.TotalAmount
                });
            }
            else
            {
                journalLines.Add(new JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = returnDoc.TotalAmount,
                    CreditAmount = 0
                });

                var inventoryByAccount = new Dictionary<Guid, decimal>();
                foreach (var line in returnDoc.Lines)
                {
                    var accountId = await GetItemInventoryAccountIdAsync(line.ItemId, cancellationToken);
                    var amount = line.Quantity * line.UnitPrice;
                    if (!inventoryByAccount.ContainsKey(accountId)) inventoryByAccount[accountId] = 0;
                    inventoryByAccount[accountId] += amount;
                }
                foreach (var item in inventoryByAccount)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = item.Key,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = item.Value
                    });
                }

                if (returnDoc.TaxAmount > 0)
                {
                    journalLines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = returnDoc.TaxAmount
                    });
                }
            }

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = returnDoc.ReturnDate,
                Description = description,
                JournalLines = journalLines
            }, userId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = returnDoc.ReturnDate
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
    }
}