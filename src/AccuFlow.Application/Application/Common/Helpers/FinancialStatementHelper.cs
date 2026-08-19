using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Common.Helpers
{
    public static class FinancialStatementHelper
    {
        public static async Task<decimal> GetAccountBalanceAsync(
            IRepository<JournalLineEntity> journalLineRepository,
            Guid accountId,
            DateTime? asOfDate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<JournalLineEntity> query = journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry!.Status == "Posted"
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted);

            if (asOfDate.HasValue)
            {
                query = query.Where(jl => jl.JournalEntry!.JournalDate <= asOfDate.Value);
            }

            var totalDebit = await query.SumAsync(jl => jl.DebitAmount, cancellationToken);
            var totalCredit = await query.SumAsync(jl => jl.CreditAmount, cancellationToken);

            return totalDebit - totalCredit;
        }

        public static async Task<decimal> GetOpeningBalanceAsync(
            IRepository<JournalLineEntity> journalLineRepository,
            Guid accountId,
            DateTime dateFrom,
            CancellationToken cancellationToken = default)
        {
            var totalDebit = await journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate < dateFrom
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.DebitAmount, cancellationToken);

            var totalCredit = await journalLineRepository.Query()
                .Include(jl => jl.JournalEntry)
                .Where(jl => jl.AccountId == accountId
                    && jl.JournalEntry!.Status == "Posted"
                    && jl.JournalEntry.JournalDate < dateFrom
                    && !jl.IsDeleted
                    && !jl.JournalEntry.IsDeleted)
                .SumAsync(jl => jl.CreditAmount, cancellationToken);

            return totalDebit - totalCredit;
        }
    }
}