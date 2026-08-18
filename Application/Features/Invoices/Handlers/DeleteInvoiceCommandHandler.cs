using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand>
    {
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteInvoiceCommandHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be deleted");
            invoice.IsDeleted = true;
            invoice.DeletedBy = request.UserId.ToString();
            invoice.DeletedAt = DateTime.UtcNow;
            foreach (var line in invoice.Lines)
            {
                line.IsDeleted = true;
                line.DeletedBy = request.UserId.ToString();
                line.DeletedAt = DateTime.UtcNow;
            }
            _invoiceRepository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}