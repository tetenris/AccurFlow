using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateInvoiceCommandHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.InvoiceId == r.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be edited");
            if (r.InvoiceType == "Sales" && !r.CustomerId.HasValue) throw new Exception("Customer is required for sales invoice");
            if (r.InvoiceType == "Purchase" && !r.SupplierId.HasValue) throw new Exception("Supplier is required for purchase invoice");
            if (!r.Lines.Any()) throw new Exception("Invoice lines are required");

            invoice.InvoiceType = r.InvoiceType;
            invoice.InvoiceDate = r.InvoiceDate;
            invoice.DueDate = r.DueDate;
            invoice.CustomerId = r.InvoiceType == "Sales" ? r.CustomerId : null;
            invoice.SupplierId = r.InvoiceType == "Purchase" ? r.SupplierId : null;
            invoice.Notes = r.Notes;
            invoice.SubTotal = 0;
            invoice.DiscountAmount = 0;
            invoice.TaxAmount = 0;
            invoice.TotalAmount = 0;
            invoice.UpdatedBy = userId.ToString();
            invoice.UpdatedAt = DateTime.UtcNow;

            foreach (var oldLine in invoice.Lines)
            {
                oldLine.IsDeleted = true;
                oldLine.DeletedBy = userId.ToString();
                oldLine.DeletedAt = DateTime.UtcNow;
            }

            foreach (var line in r.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                var entityLine = new InvoiceLineEntity
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
                };
                invoice.Lines.Add(entityLine);
                invoice.SubTotal += line.Quantity * line.UnitPrice;
                invoice.DiscountAmount += line.DiscountAmount;
                invoice.TaxAmount += line.TaxAmount;
                invoice.TotalAmount += lineTotal;
            }

            _invoiceRepository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}