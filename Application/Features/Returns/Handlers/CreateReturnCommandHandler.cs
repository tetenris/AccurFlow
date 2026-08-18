using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class CreateReturnCommandHandler : IRequestHandler<CreateReturnCommand>
    {
        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IRepository<WarehouseEntity> _warehouseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReturnCommandHandler(
            IRepository<GoodsReturnEntity> goodsReturnRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IRepository<WarehouseEntity> warehouseRepository,
            IUnitOfWork unitOfWork)
        {
            _goodsReturnRepository = goodsReturnRepository;
            _invoiceRepository = invoiceRepository;
            _warehouseRepository = warehouseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateReturnCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.InvoiceId == Guid.Empty) throw new Exception("Invoice is required");
            if (r.WarehouseId == Guid.Empty) throw new Exception("Warehouse is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.InvoiceId == r.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.InvoiceType != r.ReturnType) throw new Exception($"Return type must match invoice type ({invoice.InvoiceType})");
            if (invoice.Status == "Draft") throw new Exception("Invoice must be posted before it can be returned");
            if (invoice.Status == "Cancelled") throw new Exception("Cancelled invoice cannot be returned");

            var invoiceLineIds = invoice.Lines.Select(l => l.InvoiceLineId).ToHashSet();
            if (r.Lines.Any(l => !invoiceLineIds.Contains(l.InvoiceLineId)))
                throw new Exception("Item line does not belong to the selected invoice");

            var warehouse = await _warehouseRepository.FirstOrDefaultAsync(x => x.WarehouseId == r.WarehouseId && !x.IsDeleted, cancellationToken);
            if (warehouse == null) throw new Exception("Warehouse not found");

            var returnDoc = new GoodsReturnEntity
            {
                GoodsReturnId = Guid.NewGuid(),
                GoodsReturnNumber = await GenerateNumberAsync(r.ReturnType, cancellationToken),
                ReturnType = r.ReturnType,
                ReturnDate = r.ReturnDate,
                InvoiceId = r.InvoiceId,
                CustomerId = invoice.CustomerId,
                SupplierId = invoice.SupplierId,
                WarehouseId = r.WarehouseId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                returnDoc.Lines.Add(new GoodsReturnLineEntity
                {
                    GoodsReturnLineId = Guid.NewGuid(),
                    InvoiceLineId = line.InvoiceLineId,
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                returnDoc.SubTotal += line.Quantity * line.UnitPrice;
                returnDoc.TaxAmount += line.TaxAmount;
                returnDoc.TotalAmount += lineTotal;
            }

            if (returnDoc.Lines.Count == 0) throw new Exception("At least one item line is required");

            _goodsReturnRepository.Add(returnDoc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(string returnType, CancellationToken cancellationToken)
        {
            var prefix = returnType == "Purchase" ? "PR" : "SR";
            var last = await _goodsReturnRepository.Query()
                .Where(x => x.GoodsReturnNumber.StartsWith(prefix + "-"))
                .OrderByDescending(x => x.GoodsReturnNumber).Select(x => x.GoodsReturnNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }
    }
}