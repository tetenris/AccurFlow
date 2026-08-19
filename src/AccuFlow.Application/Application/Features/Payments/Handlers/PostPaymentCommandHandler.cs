using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.Payments.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class PostPaymentCommandHandler : IRequestHandler<PostPaymentCommand>
    {
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostPaymentCommandHandler(
            IRepository<PaymentEntity> paymentRepository,
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.Query()
                .Include(x => x.Allocations)
                .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId && !x.IsDeleted, cancellationToken);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be posted");
            if (!payment.Allocations.Any()) throw new Exception("Payment allocation is required");
            if (payment.JournalId.HasValue) throw new Exception("Payment already has journal entry");

            var journalId = await CreatePaymentJournal(payment, request.UserId, cancellationToken);
            foreach (var allocation in payment.Allocations)
            {
                var invoice = await _invoiceRepository.Query()
                    .FirstOrDefaultAsync(x => x.InvoiceId == allocation.InvoiceId && !x.IsDeleted, cancellationToken);
                if (invoice == null) continue;
                invoice.PaidAmount += allocation.AllocatedAmount;
                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? "Paid" : "PartiallyPaid";
                _invoiceRepository.Update(invoice);
            }
            payment.Status = "Posted";
            payment.JournalId = journalId;
            payment.UpdatedBy = request.UserId.ToString();
            payment.UpdatedAt = DateTime.UtcNow;
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> CreatePaymentJournal(PaymentEntity payment, Guid userId, CancellationToken cancellationToken)
        {
            var description = $"Auto journal for {payment.PaymentNumber}";
            var lines = new List<JournalLineRequest>();

            if (payment.PaymentType == "Payment")
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = payment.TotalAmount,
                    CreditAmount = 0
                });
                lines.Add(new JournalLineRequest
                {
                    AccountId = payment.CashBankAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = payment.TotalAmount
                });
            }
            else
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = payment.CashBankAccountId,
                    Description = description,
                    DebitAmount = payment.TotalAmount,
                    CreditAmount = 0
                });
                lines.Add(new JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = payment.TotalAmount
                });
            }

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = payment.PaymentDate,
                Description = description,
                JournalLines = lines
            }, userId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = payment.PaymentDate
            }, userId), cancellationToken);

            return journalId;
        }
    }
}