using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInvoiceCommandHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.InvoiceType == "Sales" && !r.CustomerId.HasValue) throw new Exception("Customer is required for sales invoice");
            if (r.InvoiceType == "Purchase" && !r.SupplierId.HasValue) throw new Exception("Supplier is required for purchase invoice");
            if (!r.Lines.Any()) throw new Exception("Invoice lines are required");

            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = await GenerateNumber(r.InvoiceType, cancellationToken),
                InvoiceType = r.InvoiceType,
                InvoiceDate = r.InvoiceDate,
                DueDate = r.DueDate,
                CustomerId = r.CustomerId,
                SupplierId = r.SupplierId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    AccountId = line.AccountId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                invoice.SubTotal += line.Quantity * line.UnitPrice;
                invoice.DiscountAmount += line.DiscountAmount;
                invoice.TaxAmount += line.TaxAmount;
                invoice.TotalAmount += lineTotal;
            }

            _invoiceRepository.Add(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumber(string invoiceType, CancellationToken cancellationToken)
        {
            var prefix = invoiceType == "Purchase" ? "PI" : "SI";
            var last = await _invoiceRepository.Query()
                .Where(x => x.InvoiceNumber.StartsWith(prefix + "-"))
                .OrderByDescending(x => x.InvoiceNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = last == null ? 1 : int.Parse(last.InvoiceNumber[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }
    }
}