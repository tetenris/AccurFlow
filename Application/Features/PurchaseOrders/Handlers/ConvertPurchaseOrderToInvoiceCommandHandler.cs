using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class ConvertPurchaseOrderToInvoiceCommandHandler : IRequestHandler<ConvertPurchaseOrderToInvoiceCommand>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConvertPurchaseOrderToInvoiceCommandHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _poRepository = poRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ConvertPurchaseOrderToInvoiceCommand request, CancellationToken cancellationToken)
        {
            var po = await _poRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.PurchaseInvoiceId.HasValue) throw new Exception("Purchase order already converted");

            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}",
                InvoiceType = "Purchase",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                SupplierId = po.SupplierId,
                Status = "Draft",
                SubTotal = po.SubTotal,
                TaxAmount = po.TaxAmount,
                TotalAmount = po.TotalAmount,
                CreatedBy = request.UserId.ToString()
            };
            foreach (var line in po.Lines)
            {
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = line.LineTotal,
                    CreatedBy = request.UserId.ToString()
                });
            }
            po.PurchaseInvoiceId = invoice.InvoiceId;
            po.Status = "Converted";
            _invoiceRepository.Add(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}