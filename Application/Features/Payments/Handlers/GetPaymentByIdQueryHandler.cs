using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payments.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Payment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDetailViewModel?>
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;

        public GetPaymentByIdQueryHandler(IRepository<PaymentEntity> paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDetailViewModel?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            return await _paymentRepository.Query()
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.CashBankAccount)
                .Include(x => x.JournalEntry)
                .Include(x => x.Allocations.Where(a => !a.IsDeleted))
                    .ThenInclude(x => x.Invoice)
                .Where(x => x.PaymentId == request.PaymentId && !x.IsDeleted)
                .Select(x => new PaymentDetailViewModel
                {
                    PaymentId = x.PaymentId,
                    PaymentNumber = x.PaymentNumber,
                    PaymentType = x.PaymentType,
                    PaymentDate = x.PaymentDate,
                    CustomerId = x.CustomerId,
                    SupplierId = x.SupplierId,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    CashBankAccountId = x.CashBankAccountId,
                    CashBankAccountName = x.CashBankAccount.AccountName,
                    PaymentMethod = x.PaymentMethod,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    JournalId = x.JournalId,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    ReferenceNumber = x.ReferenceNumber,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Allocations = x.Allocations.Where(a => !a.IsDeleted).Select(a => new PaymentAllocationViewModel
                    {
                        PaymentAllocationId = a.PaymentAllocationId,
                        InvoiceId = a.InvoiceId,
                        InvoiceNumber = a.Invoice.InvoiceNumber,
                        AllocatedAmount = a.AllocatedAmount
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}