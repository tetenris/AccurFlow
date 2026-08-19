using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class ConvertPurchaseRequestCommandHandler : IRequestHandler<ConvertPurchaseRequestCommand, Guid>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConvertPurchaseRequestCommandHandler(
            IRepository<PurchaseRequestEntity> prRepository,
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _prRepository = prRepository;
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(ConvertPurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var pr = await _prRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.PurchaseRequestId == r.PurchaseRequestId && !x.IsDeleted, cancellationToken);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Approved") throw new Exception("Only approved purchase request can be converted");
            if (pr.PurchaseOrderId.HasValue) throw new Exception("Purchase request already converted to purchase order");
            if (r.SupplierId == Guid.Empty) throw new Exception("Supplier is required");

            var po = new PurchaseOrderEntity
            {
                PurchaseOrderId = Guid.NewGuid(),
                PurchaseOrderNumber = await GeneratePoNumberAsync(cancellationToken),
                OrderDate = DateTime.Today,
                ExpectedDate = r.ExpectedDate,
                SupplierId = r.SupplierId,
                Status = "Draft",
                Notes = $"Created from purchase request {pr.PurchaseRequestNumber}" + (string.IsNullOrWhiteSpace(pr.Notes) ? "" : $" - {pr.Notes}"),
                CreatedBy = userId.ToString()
            };
            foreach (var line in pr.Lines.Where(l => !l.IsDeleted))
            {
                if (line.Quantity <= 0) continue;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = line.LineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += line.LineTotal;
            }
            if (po.Lines.Count == 0) throw new Exception("Purchase request has no items to convert");

            pr.PurchaseOrderId = po.PurchaseOrderId;
            pr.Status = "Converted";
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = userId.ToString();
            _poRepository.Add(po);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return po.PurchaseOrderId;
        }

        private async Task<string> GeneratePoNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _poRepository.Query()
                .Where(x => x.PurchaseOrderNumber.StartsWith("PO-"))
                .OrderByDescending(x => x.PurchaseOrderNumber)
                .Select(x => x.PurchaseOrderNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = last == null ? 1 : int.Parse(last[3..]) + 1;
            return $"PO-{next:D5}";
        }
    }
}