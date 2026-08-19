using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Invoices.Commands;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Invoices.Handlers
{
    public class PostInvoiceCommandHandler : IRequestHandler<PostInvoiceCommand>
    {
        private static readonly Guid AccountsReceivableAccountId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        private static readonly Guid AccountsPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid TaxPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        private static readonly Guid SalesRevenueAccountId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        private static readonly Guid CostOfGoodsSoldAccountId = Guid.Parse("50000000-0000-0000-0000-000000000002");

        private readonly IRepository<InvoiceEntity> _invoiceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostInvoiceCommandHandler(
            IRepository<InvoiceEntity> invoiceRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _invoiceRepository = invoiceRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _invoiceRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.InvoiceId == request.InvoiceId && !x.IsDeleted, cancellationToken);
            if (invoice == null) throw new Exception("Invoice not found");
            if (invoice.Status != "Draft") throw new Exception("Only draft invoice can be posted");
            if (invoice.JournalId.HasValue) throw new Exception("Invoice already has journal entry");

            var journalId = await CreateInvoiceJournal(invoice, request.UserId, cancellationToken);
            invoice.Status = "Posted";
            invoice.JournalId = journalId;
            invoice.UpdatedBy = request.UserId.ToString();
            invoice.UpdatedAt = DateTime.UtcNow;
            _invoiceRepository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Guid> CreateInvoiceJournal(InvoiceEntity invoice, Guid userId, CancellationToken cancellationToken)
        {
            var lines = new List<JournalLineRequest>();
            var description = $"Auto journal for {invoice.InvoiceNumber}";

            if (invoice.InvoiceType == "Purchase")
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = CostOfGoodsSoldAccountId,
                    Description = description,
                    DebitAmount = invoice.SubTotal - invoice.DiscountAmount,
                    CreditAmount = 0
                });

                if (invoice.TaxAmount > 0)
                {
                    lines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = invoice.TaxAmount,
                        CreditAmount = 0
                    });
                }

                lines.Add(new JournalLineRequest
                {
                    AccountId = AccountsPayableAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = invoice.TotalAmount
                });
            }
            else
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = AccountsReceivableAccountId,
                    Description = description,
                    DebitAmount = invoice.TotalAmount,
                    CreditAmount = 0
                });

                lines.Add(new JournalLineRequest
                {
                    AccountId = SalesRevenueAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = invoice.SubTotal - invoice.DiscountAmount
                });

                if (invoice.TaxAmount > 0)
                {
                    lines.Add(new JournalLineRequest
                    {
                        AccountId = TaxPayableAccountId,
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = invoice.TaxAmount
                    });
                }
            }

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = invoice.InvoiceDate,
                Description = description,
                JournalLines = lines
            }, userId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = invoice.InvoiceDate
            }, userId), cancellationToken);

            return journalId;
        }
    }
}