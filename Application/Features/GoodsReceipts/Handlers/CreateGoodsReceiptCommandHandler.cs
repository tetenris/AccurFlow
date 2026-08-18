using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class CreateGoodsReceiptCommandHandler : IRequestHandler<CreateGoodsReceiptCommand>
    {
        private readonly IRepository<GoodsReceiptEntity> _grnRepository;
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IRepository<WarehouseEntity> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateGoodsReceiptCommandHandler(
            IRepository<GoodsReceiptEntity> grnRepository,
            IRepository<PurchaseOrderEntity> poRepository,
            IRepository<WarehouseEntity> warehouseRepository,
            IUnitOfWork unitOfWork)
        {
            _grnRepository = grnRepository;
            _poRepository = poRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.PurchaseOrderId == Guid.Empty) throw new Exception("Purchase order is required");
            if (r.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var po = await _poRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == r.PurchaseOrderId && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Approved") throw new Exception("Only approved purchase order can be received");

            var poLineIds = po.Lines.Select(l => l.PurchaseOrderLineId).ToHashSet();
            if (r.Lines.Any(l => !poLineIds.Contains(l.PurchaseOrderLineId)))
                throw new Exception("Item line does not belong to the selected purchase order");

            var warehouse = await _warehouseRepository.Query()
                .FirstOrDefaultAsync(x => x.WarehouseId == r.WarehouseId && !x.IsDeleted, cancellationToken);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var grn = new GoodsReceiptEntity
            {
                GoodsReceiptId = Guid.NewGuid(),
                GoodsReceiptNumber = await GenerateNumberAsync(cancellationToken),
                ReceiptDate = r.ReceiptDate,
                PurchaseOrderId = r.PurchaseOrderId,
                SupplierId = po.SupplierId,
                WarehouseId = r.WarehouseId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                grn.Lines.Add(new GoodsReceiptLineEntity
                {
                    GoodsReceiptLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    PurchaseOrderLineId = line.PurchaseOrderLineId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                grn.SubTotal += line.Quantity * line.UnitPrice;
                grn.TaxAmount += line.TaxAmount;
                grn.TotalAmount += lineTotal;
            }

            if (grn.Lines.Count == 0) throw new Exception("At least one item line is required");

            _grnRepository.Add(grn);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _grnRepository.Query()
                .Where(x => x.GoodsReceiptNumber.StartsWith("GRN-"))
                .OrderByDescending(x => x.GoodsReceiptNumber)
                .Select(x => x.GoodsReceiptNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"GRN-{next:D5}";
        }
    }
}