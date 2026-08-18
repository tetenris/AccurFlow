using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelInvoiceCommandHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.Query()
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.PaidAmount > 0) throw new Exception("Paid invoice cannot be cancelled");
            invoice.Status = "Cancelled";
            invoice.UpdatedBy = request.UserId.ToString();
            invoice.UpdatedAt = DateTime.UtcNow;
            _invoiceRepository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}