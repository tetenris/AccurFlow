using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Quotation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class UpdateQuotationCommandHandler : IRequestHandler<UpdateQuotationCommand>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateQuotationCommandHandler(
            IRepository<SalesQuotationEntity> quotationRepository,
            IUnitOfWork unitOfWork)
        {
            _quotationRepository = quotationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateQuotationCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var quote = await _quotationRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.SalesQuotationId == r.SalesQuotationId && !x.IsDeleted, cancellationToken);
            if (quote == null) throw new Exception("Quotation not found");
            if (quote.Status != "Draft") throw new Exception("Only draft quotation can be edited");

            quote.QuotationDate = r.QuotationDate;
            quote.ValidUntil = r.ValidUntil;
            quote.CustomerId = r.CustomerId;
            quote.Notes = r.Notes;
            foreach (var existing in quote.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            quote.SubTotal = 0; quote.DiscountAmount = 0; quote.TaxAmount = 0; quote.TotalAmount = 0;
            FillTotals(quote, r.Lines, userId);
            quote.UpdatedAt = DateTime.UtcNow;
            quote.UpdatedBy = userId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private void FillTotals(SalesQuotationEntity quote, IEnumerable<QuotationLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                quote.Lines.Add(new SalesQuotationLineEntity
                {
                    SalesQuotationLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                quote.SubTotal += line.Quantity * line.UnitPrice;
                quote.DiscountAmount += line.DiscountAmount;
                quote.TaxAmount += line.TaxAmount;
                quote.TotalAmount += lineTotal;
            }
        }
    }
}