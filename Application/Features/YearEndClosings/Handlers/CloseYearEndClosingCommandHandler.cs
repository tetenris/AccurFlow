using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.YearEndClosings.Commands;
using AccuFlow.Application.Features.YearEndClosings.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using AccuFlow.Models.YearEndClosing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.YearEndClosings.Handlers
{
    public class CloseYearEndClosingCommandHandler : IRequestHandler<CloseYearEndClosingCommand, YearEndClosingHistoryItem>
    {
        private static readonly Guid RetainedEarningsAccountId = Guid.Parse("30000000-0000-0000-0000-000000000003");

        private readonly IRepository<YearEndClosingEntity> _yearEndClosingRepository;
        private readonly IRepository<JournalEntryEntity> _journalRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public CloseYearEndClosingCommandHandler(
            IRepository<YearEndClosingEntity> yearEndClosingRepository,
            IRepository<JournalEntryEntity> journalRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _yearEndClosingRepository = yearEndClosingRepository;
            _journalRepository = journalRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<YearEndClosingHistoryItem> Handle(CloseYearEndClosingCommand request, CancellationToken cancellationToken)
        {
            var alreadyClosed = await _yearEndClosingRepository.AnyAsync(x => x.FiscalYear == request.Request.FiscalYear && !x.IsDeleted, cancellationToken);
            if (alreadyClosed) throw new Exception($"Fiscal year {request.Request.FiscalYear} is already closed");

            var preview = await _mediator.Send(new GetYearEndClosingPreviewQuery(request.Request), cancellationToken);
            var endOfYear = new DateTime(request.Request.FiscalYear, 12, 31);
            var description = $"Year-end closing for {request.Request.FiscalYear}";

            var lines = new List<JournalLineRequest>();
            foreach (var revenue in preview.RevenueAccounts)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = revenue.AccountId,
                    Description = description,
                    DebitAmount = revenue.Balance,
                    CreditAmount = 0
                });
            }
            foreach (var expense in preview.ExpenseAccounts)
            {
                lines.Add(new JournalLineRequest
                {
                    AccountId = expense.AccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = expense.Balance
                });
            }
            lines.Add(new JournalLineRequest
            {
                AccountId = RetainedEarningsAccountId,
                Description = description,
                DebitAmount = preview.TotalExpense,
                CreditAmount = preview.TotalRevenue
            });

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = endOfYear,
                Description = description,
                JournalLines = lines
            }, request.UserId), cancellationToken);
            await _mediator.Send(new PostJournalCommand(new PostJournalRequest { JournalId = journalId, PostedDate = endOfYear }, request.UserId), cancellationToken);

            var closing = new YearEndClosingEntity
            {
                ClosingId = Guid.NewGuid(),
                FiscalYear = request.Request.FiscalYear,
                ClosingDate = endOfYear,
                ClosingJournalId = journalId,
                CreatedBy = request.UserId.ToString()
            };
            _yearEndClosingRepository.Add(closing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var journalNumber = await _journalRepository.Query()
                .Where(x => x.JournalId == journalId)
                .Select(x => x.JournalNumber)
                .FirstOrDefaultAsync(cancellationToken);
            return new YearEndClosingHistoryItem
            {
                ClosingId = closing.ClosingId,
                FiscalYear = closing.FiscalYear,
                ClosingDate = closing.ClosingDate,
                ClosingJournalId = journalId,
                JournalNumber = journalNumber
            };
        }
    }
}