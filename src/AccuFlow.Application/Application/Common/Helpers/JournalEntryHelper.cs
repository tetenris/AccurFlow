using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Common.Helpers
{
    public static class JournalEntryHelper
    {
        public static async Task<string> GenerateJournalNumberAsync(
            IRepository<JournalEntryEntity> journalRepository,
            DateTime journalDate,
            string? journalType = null,
            CancellationToken cancellationToken = default)
        {
            var datePrefix = journalDate.ToString("yyyyMMdd");
            var type = string.IsNullOrEmpty(journalType) ? "General" : journalType;
            var typeCode = type switch
            {
                "Adjustment" => "JA",
                "Memo" => "JM",
                _ => "JE"
            };
            var prefix = $"{typeCode}-{datePrefix}-";

            var lastJournal = await journalRepository.Query()
                .Where(x => x.JournalNumber.StartsWith(prefix))
                .OrderByDescending(x => x.JournalNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastJournal != null)
            {
                var lastNumber = lastJournal.JournalNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out int number))
                {
                    return $"{prefix}{(number + 1):D4}";
                }
            }

            return $"{prefix}0001";
        }

        public static async Task<List<string>> ValidateAsync(
            IRepository<ChartOfAccountEntity> coaRepository,
            CreateJournalEntryRequest request,
            CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            if (request.JournalDate > DateTime.Now)
            {
                errors.Add("Journal date cannot be in the future");
            }

            if (request.JournalLines.Count < 2)
            {
                errors.Add("At least 2 journal lines are required");
            }

            var totalDebit = request.JournalLines.Sum(x => x.DebitAmount);
            var totalCredit = request.JournalLines.Sum(x => x.CreditAmount);

            if (totalDebit != totalCredit)
            {
                errors.Add($"Journal is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");
            }

            foreach (var line in request.JournalLines)
            {
                if (line.DebitAmount < 0 || line.CreditAmount < 0)
                {
                    errors.Add("Amounts must be positive");
                }

                if (line.DebitAmount > 0 && line.CreditAmount > 0)
                {
                    errors.Add("A line cannot have both debit and credit amounts");
                }

                if (line.DebitAmount == 0 && line.CreditAmount == 0)
                {
                    errors.Add("A line must have either debit or credit amount");
                }
            }

            var accountIds = request.JournalLines.Select(x => x.AccountId).Distinct().ToList();
            var accounts = await coaRepository.Query()
                .Where(x => accountIds.Contains(x.AccountId) && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var accountId in accountIds)
            {
                var account = accounts.FirstOrDefault(x => x.AccountId == accountId);
                if (account == null)
                {
                    errors.Add("Account not found");
                }
                else
                {
                    if (!account.IsActive)
                    {
                        errors.Add($"Account {account.AccountCode} is not active");
                    }

                    if (account.IsHeader)
                    {
                        errors.Add($"Account {account.AccountCode} is a header account and cannot be used in journal entries");
                    }
                }
            }

            return errors;
        }
    }
}