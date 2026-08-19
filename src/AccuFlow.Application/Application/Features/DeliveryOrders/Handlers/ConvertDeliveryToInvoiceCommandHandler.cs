using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class ConvertDeliveryToInvoiceCommandHandler : IRequestHandler<ConvertDeliveryToInvoiceCommand>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConvertDeliveryToInvoiceCommandHandler(
            IRepository<DeliveryOrderEntity> deliveryRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ConvertDeliveryToInvoiceCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Posted") throw new Exception("Only posted delivery order can be converted to invoice");
            if (delivery.InvoiceId.HasValue) throw new Exception("Delivery order already converted to invoice");

            var invoice = new InvoiceEntity
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken),
                InvoiceType = "Sales",
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                CustomerId = delivery.CustomerId,
                Status = "Draft",
                SubTotal = delivery.TotalAmount,
                TotalAmount = delivery.TotalAmount,
                Notes = $"Auto created from delivery {delivery.DeliveryNumber}",
                CreatedBy = request.UserId.ToString()
            };
            foreach (var line in delivery.Lines)
            {
                invoice.Lines.Add(new InvoiceLineEntity
                {
                    InvoiceLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    LineTotal = line.LineTotal,
                    CreatedBy = request.UserId.ToString()
                });
            }
            delivery.InvoiceId = invoice.InvoiceId;
            delivery.Status = "Invoiced";
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = request.UserId.ToString();
            _invoiceRepository.Add(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _invoiceRepository.Query()
                .Where(x => x.InvoiceNumber.StartsWith("SI-"))
                .OrderByDescending(x => x.InvoiceNumber)
                .Select(x => x.InvoiceNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = last == null ? 1 : int.Parse(last[3..]) + 1;
            return $"SI-{next:D5}";
        }
    }
}