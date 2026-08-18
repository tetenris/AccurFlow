using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePurchaseOrderCommandHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var po = await _poRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == r.PurchaseOrderId && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be edited");
            if (!r.Lines.Any()) throw new Exception("Purchase order lines are required");

            po.OrderDate = r.OrderDate;
            po.ExpectedDate = r.ExpectedDate;
            po.SupplierId = r.SupplierId;
            po.Notes = r.Notes;
            po.SubTotal = 0;
            po.TaxAmount = 0;
            po.TotalAmount = 0;
            po.UpdatedBy = userId.ToString();
            po.UpdatedAt = DateTime.UtcNow;

            foreach (var oldLine in po.Lines)
            {
                oldLine.IsDeleted = true;
                oldLine.DeletedBy = userId.ToString();
                oldLine.DeletedAt = DateTime.UtcNow;
            }

            foreach (var line in r.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += lineTotal;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}