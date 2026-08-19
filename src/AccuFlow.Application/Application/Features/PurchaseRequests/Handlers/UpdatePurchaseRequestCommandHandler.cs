using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.PurchaseRequest;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class UpdatePurchaseRequestCommandHandler : IRequestHandler<UpdatePurchaseRequestCommand>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePurchaseRequestCommandHandler(
            IRepository<PurchaseRequestEntity> prRepository,
            IUnitOfWork unitOfWork)
        {
            _prRepository = prRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdatePurchaseRequestCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var pr = await _prRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.PurchaseRequestId == r.PurchaseRequestId && !x.IsDeleted, cancellationToken);
            if (pr == null) throw new Exception("Purchase request not found");
            if (pr.Status != "Draft") throw new Exception("Only draft purchase request can be edited");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            pr.RequestDate = r.RequestDate;
            pr.RequiredDate = r.RequiredDate;
            pr.RequestedBy = string.IsNullOrWhiteSpace(r.RequestedBy) ? pr.RequestedBy : r.RequestedBy;
            pr.Department = r.Department;
            pr.Notes = r.Notes;
            pr.UpdatedAt = DateTime.UtcNow;
            pr.UpdatedBy = userId.ToString();
            foreach (var existing in pr.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            pr.SubTotal = 0; pr.TaxAmount = 0; pr.TotalAmount = 0;
            FillTotals(pr, r.Lines, userId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private void FillTotals(PurchaseRequestEntity pr, IEnumerable<CreatePurchaseRequestLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                pr.Lines.Add(new PurchaseRequestLineEntity
                {
                    PurchaseRequestLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                pr.SubTotal += line.Quantity * line.UnitPrice;
                pr.TaxAmount += line.TaxAmount;
                pr.TotalAmount += lineTotal;
            }
        }
    }
}