using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.GeneralLedger;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GeneralLedgers.Handlers
{
    public class GetLedgerQueryHandler : IRequestHandler<GetLedgerQuery, AccountLedgerViewModel>
    {
        private readonly IRepository<JournalLineEntity> _journalLineRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetLedgerQueryHandler(
            IRepository<JournalLineEntity> journalLineRepository,
            IRepository<ChartOfAccountEntity> coaRepository)
        {
            _journalLineRepository = journalLineRepository;
            _coaRepository = coaRepository;
        }

        public async Task<AccountLedgerViewModel> Handle(GetLedgerQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var account = await _coaRepository.FirstOrDefaultAsync(
                a => a.AccountId == r.AccountId,
                cancellationToken);

            if (account == null)
                throw new Exception("Account not found");

            IQueryable<JournalLineEntity> query = _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                    .ThenInclude(je => je!.PostedByUser)
                .Where(jl => jl.AccountId == r.AccountId
                    && (jl.JournalEntry!.Status == "Posted" || jl.JournalEntry.Status == "Reversed")
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted);

            if (r.DateFrom.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate >= r.DateFrom.Value);
            }

            if (r.DateTo.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate <= r.DateTo.Value);
            }

            var entries = await query
                .OrderBy(jl => jl.JournalEntry!.JournalDate)
                .ThenBy(jl => jl.JournalEntry!.JournalNumber)
                .Select(jl => new LedgerEntryViewModel
                {
                    JournalLineId = jl.JournalLineId,
                    JournalId = jl.JournalId,
                    JournalNumber = jl.JournalEntry!.JournalNumber,
                    JournalDate = jl.JournalEntry.JournalDate,
                    Description = jl.Description,
                    DebitAmount = jl.DebitAmount,
                    CreditAmount = jl.CreditAmount,
                    PostedBy = jl.JournalEntry.PostedByUser != null ? jl.JournalEntry.PostedByUser.FullName : "",
                    PostedDate = jl.JournalEntry.PostedDate ?? DateTime.Now,
                    Status = jl.JournalEntry.Status,
                    IsReversal = jl.JournalEntry.OriginalJournalId != null
                })
                .ToListAsync(cancellationToken);

            decimal openingBalance = 0;
            if (r.DateFrom.HasValue)
            {
                openingBalance = await GetOpeningBalanceAsync(r.AccountId, r.DateFrom.Value, cancellationToken);
            }

            decimal runningBalance = openingBalance;
            var isDebitBalance = GeneralLedgerHelper.IsDebitBalance(account.AccountType);

            foreach (var entry in entries)
            {
                runningBalance += isDebitBalance
                    ? entry.DebitAmount - entry.CreditAmount
                    : entry.CreditAmount - entry.DebitAmount;
                entry.RunningBalance = runningBalance;
            }

            var totalDebit = entries.Sum(e => e.DebitAmount);
            var totalCredit = entries.Sum(e => e.CreditAmount);

            return new AccountLedgerViewModel
            {
                AccountId = account.AccountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                OpeningBalance = openingBalance,
                ClosingBalance = runningBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Entries = entries
            };
        }

        private async Task<decimal> GetOpeningBalanceAsync(Guid accountId, DateTime dateFrom, CancellationToken cancellationToken)
        {
            var account = await _coaRepository.FirstOrDefaultAsync(
                a => a.AccountId == accountId,
                cancellationToken);

            if (account == null)
                return 0;

            var totalDebit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate < dateFrom
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.DebitAmount, cancellationToken);

            var totalCredit = await _journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate < dateFrom
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.CreditAmount, cancellationToken);

            return GeneralLedgerHelper.CalculateBalance(account.AccountType, totalDebit, totalCredit);
        }
    }
}