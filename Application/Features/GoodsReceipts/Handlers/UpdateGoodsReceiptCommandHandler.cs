using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GoodsReceipt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class UpdateGoodsReceiptCommandHandler : IRequestHandler<UpdateGoodsReceiptCommand>
    {
        private readonly IRepository<GoodsReceiptEntity> _grnRepository;
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateGoodsReceiptCommandHandler(
            IRepository<GoodsReceiptEntity> grnRepository,
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _grnRepository = grnRepository;
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateGoodsReceiptCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var grn = await _grnRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.GoodsReceiptId == r.GoodsReceiptId && !x.IsDeleted, cancellationToken);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be edited");

            var po = await _poRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == r.PurchaseOrderId && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Approved") throw new Exception("Only approved purchase order can be received");

            grn.PurchaseOrderId = r.PurchaseOrderId;
            grn.SupplierId = po.SupplierId;
            grn.WarehouseId = r.WarehouseId;
            grn.ReceiptDate = r.ReceiptDate;
            grn.Notes = r.Notes;
            grn.SubTotal = 0;
            grn.TaxAmount = 0;
            grn.TotalAmount = 0;
            grn.UpdatedAt = DateTime.UtcNow;
            grn.UpdatedBy = userId.ToString();

            foreach (var existing in grn.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}