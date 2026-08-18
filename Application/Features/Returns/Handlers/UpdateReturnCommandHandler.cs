using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class UpdateReturnCommandHandler : IRequestHandler<UpdateReturnCommand>
    {
        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateReturnCommandHandler(
            IRepository<GoodsReturnEntity> goodsReturnRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _goodsReturnRepository = goodsReturnRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateReturnCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var returnDoc = await _goodsReturnRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.GoodsReturnId == r.GoodsReturnId && !x.IsDeleted, cancellationToken);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be edited");

            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.InvoiceId == r.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.InvoiceType != r.ReturnType) throw new Exception($"Return type must match invoice type ({invoice.InvoiceType})");

            returnDoc.ReturnType = r.ReturnType;
            returnDoc.ReturnDate = r.ReturnDate;
            returnDoc.InvoiceId = r.InvoiceId;
            returnDoc.CustomerId = invoice.CustomerId;
            returnDoc.SupplierId = invoice.SupplierId;
            returnDoc.WarehouseId = r.WarehouseId;
            returnDoc.Notes = r.Notes;
            returnDoc.SubTotal = 0;
            returnDoc.TaxAmount = 0;
            returnDoc.TotalAmount = 0;
            returnDoc.UpdatedAt = DateTime.UtcNow;
            returnDoc.UpdatedBy = userId.ToString();

            foreach (var existing in returnDoc.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

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

            _goodsReturnRepository.Update(returnDoc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}