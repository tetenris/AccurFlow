using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesQuotations.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Quotation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesQuotations.Handlers
{
    public class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand>
    {
        private readonly IRepository<SalesQuotationEntity> _quotationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateQuotationCommandHandler(
            IRepository<SalesQuotationEntity> quotationRepository,
            IUnitOfWork unitOfWork)
        {
            _quotationRepository = quotationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.CustomerId == Guid.Empty) throw new Exception("Customer is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var quote = new SalesQuotationEntity
            {
                SalesQuotationId = Guid.NewGuid(),
                QuotationNumber = await GenerateNumberAsync(cancellationToken),
                QuotationDate = r.QuotationDate,
                ValidUntil = r.ValidUntil,
                CustomerId = r.CustomerId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(quote, r.Lines, userId);
            _quotationRepository.Add(quote);
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

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _quotationRepository.Query()
                .Where(x => x.QuotationNumber.StartsWith("QT-"))
                .OrderByDescending(x => x.QuotationNumber)
                .Select(x => x.QuotationNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"QT-{next:D5}";
        }
    }
}