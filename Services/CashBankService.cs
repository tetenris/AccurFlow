using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.CashBank;
using AccuFlow.Models.JournalEntry;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface ICashBankService : IBaseService
    {
        Task<BaseDatatableResponse> GetTransfersAsync(DataTableTransferRequest request);
        Task<TransferDetailViewModel?> GetTransferDetailAsync(Guid id);
        Task CreateTransferAsync(CreateTransferRequest request, Guid userId);
        Task UpdateTransferAsync(UpdateTransferRequest request, Guid userId);
        Task PostTransferAsync(Guid id, Guid userId);
        Task DeleteTransferAsync(Guid id, Guid userId);

        Task<BaseDatatableResponse> GetReconciliationsAsync(DataTableReconciliationRequest request);
        Task<ReconciliationDetailViewModel?> GetReconciliationDetailAsync(Guid id);
        Task<List<ReconciliationLineViewModel>> GetBankStatementAsync(Guid accountId, DateTime asOfDate);
        Task CreateReconciliationAsync(CreateReconciliationRequest request, Guid userId);
        Task UpdateReconciliationAsync(UpdateReconciliationRequest request, Guid userId);
        Task PostReconciliationAsync(Guid id, Guid userId);
        Task DeleteReconciliationAsync(Guid id, Guid userId);
    }

    public class CashBankService : BaseService, ICashBankService
    {
        private readonly IJournalEntryService _journalEntryService;

        public CashBankService(
            AppDbContext dbContext,
            IJournalEntryService journalEntryService) : base(dbContext)
        {
            _journalEntryService = journalEntryService;
        }

        #region Transfers

        public async Task<BaseDatatableResponse> GetTransfersAsync(DataTableTransferRequest request)
        {
            var query = _dbContext.CashBankTransfers
                .Include(x => x.FromAccount)
                .Include(x => x.ToAccount)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.DateFrom.HasValue) query = query.Where(x => x.TransferDate >= request.DateFrom.Value.Date);
            if (request.DateTo.HasValue) query = query.Where(x => x.TransferDate <= request.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.TransferNumber.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.TransferDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new TransferViewModel
                {
                    TransferId = x.TransferId,
                    TransferNumber = x.TransferNumber,
                    TransferDate = x.TransferDate,
                    FromAccountName = x.FromAccount != null ? x.FromAccount.AccountName : string.Empty,
                    ToAccountName = x.ToAccount != null ? x.ToAccount.AccountName : string.Empty,
                    Amount = x.Amount,
                    Status = x.Status
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<TransferDetailViewModel?> GetTransferDetailAsync(Guid id)
        {
            return await _dbContext.CashBankTransfers
                .Include(x => x.FromAccount)
                .Include(x => x.ToAccount)
                .Include(x => x.JournalEntry)
                .Where(x => x.TransferId == id && !x.IsDeleted)
                .Select(x => new TransferDetailViewModel
                {
                    TransferId = x.TransferId,
                    TransferNumber = x.TransferNumber,
                    TransferDate = x.TransferDate,
                    FromAccountId = x.FromAccountId,
                    ToAccountId = x.ToAccountId,
                    FromAccountName = x.FromAccount != null ? x.FromAccount.AccountName : string.Empty,
                    ToAccountName = x.ToAccount != null ? x.ToAccount.AccountName : string.Empty,
                    Amount = x.Amount,
                    Description = x.Description,
                    ReferenceNumber = x.ReferenceNumber,
                    Status = x.Status,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft"
                }).FirstOrDefaultAsync();
        }

        public async Task CreateTransferAsync(CreateTransferRequest request, Guid userId)
        {
            ValidateTransfer(request.FromAccountId, request.ToAccountId, request.Amount);
            await ValidateTransferAccountsAsync(request.FromAccountId, request.ToAccountId);

            var transfer = new CashBankTransferEntity
            {
                TransferId = Guid.NewGuid(),
                TransferNumber = await GenerateNumberAsync("TRF"),
                TransferDate = request.TransferDate,
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                Description = request.Description,
                ReferenceNumber = request.ReferenceNumber,
                Status = "Draft",
                CreatedBy = userId.ToString()
            };

            _dbContext.CashBankTransfers.Add(transfer);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTransferAsync(UpdateTransferRequest request, Guid userId)
        {
            var transfer = await _dbContext.CashBankTransfers.FirstOrDefaultAsync(x => x.TransferId == request.TransferId && !x.IsDeleted);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be edited");

            ValidateTransfer(request.FromAccountId, request.ToAccountId, request.Amount);
            await ValidateTransferAccountsAsync(request.FromAccountId, request.ToAccountId);

            transfer.FromAccountId = request.FromAccountId;
            transfer.ToAccountId = request.ToAccountId;
            transfer.TransferDate = request.TransferDate;
            transfer.Amount = request.Amount;
            transfer.Description = request.Description;
            transfer.ReferenceNumber = request.ReferenceNumber;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task PostTransferAsync(Guid id, Guid userId)
        {
            var transfer = await _dbContext.CashBankTransfers.FirstOrDefaultAsync(x => x.TransferId == id && !x.IsDeleted);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status == "Posted") throw new Exception("Transfer is already posted");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be posted");

            var description = $"Auto journal for {transfer.TransferNumber}";
            var lines = new List<JournalLineRequest>
            {
                new JournalLineRequest
                {
                    AccountId = transfer.ToAccountId,
                    Description = description,
                    DebitAmount = transfer.Amount,
                    CreditAmount = 0
                },
                new JournalLineRequest
                {
                    AccountId = transfer.FromAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = transfer.Amount
                }
            };

            var journalId = await _journalEntryService.CreateAsync(new CreateJournalEntryRequest
            {
                JournalDate = transfer.TransferDate,
                Description = description,
                JournalLines = lines
            }, userId);

            await _journalEntryService.PostAsync(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = transfer.TransferDate
            }, userId);

            transfer.JournalId = journalId;
            transfer.Status = "Posted";
            transfer.PostedDate = DateTime.UtcNow;
            transfer.PostedBy = userId;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTransferAsync(Guid id, Guid userId)
        {
            var transfer = await _dbContext.CashBankTransfers.FirstOrDefaultAsync(x => x.TransferId == id && !x.IsDeleted);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be deleted");

            transfer.IsDeleted = true;
            transfer.DeletedAt = DateTime.UtcNow;
            transfer.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        private async Task ValidateTransferAccountsAsync(Guid fromAccountId, Guid toAccountId)
        {
            var accountIds = new[] { fromAccountId, toAccountId };
            var valid = await _dbContext.ChartOfAccounts
                .CountAsync(x => accountIds.Contains(x.AccountId) && (x.AccountUsage == 1 || x.AccountUsage == 2) && !x.IsHeader && x.IsActive && !x.IsDeleted);
            if (valid != 2) throw new Exception("Both accounts must be active cash/bank accounts");
        }

        private void ValidateTransfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            if (fromAccountId == toAccountId) throw new Exception("From and To accounts must be different");
            if (amount <= 0) throw new Exception("Amount must be greater than zero");
        }

        #endregion

        #region Reconciliations

        public async Task<BaseDatatableResponse> GetReconciliationsAsync(DataTableReconciliationRequest request)
        {
            var query = _dbContext.BankReconciliations
                .Include(x => x.Account)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => x.ReconciliationNumber.ToLower().Contains(search) || x.Account != null && x.Account.AccountName.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.StatementDate)
                .Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ReconciliationViewModel
                {
                    ReconciliationId = x.ReconciliationId,
                    ReconciliationNumber = x.ReconciliationNumber,
                    AccountName = x.Account != null ? x.Account.AccountName : string.Empty,
                    StatementDate = x.StatementDate,
                    StatementEndingBalance = x.StatementEndingBalance,
                    GlEndingBalance = x.GlEndingBalance,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    ClearedCount = x.Lines.Count(l => l.IsCleared)
                }).ToListAsync();

            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<ReconciliationDetailViewModel?> GetReconciliationDetailAsync(Guid id)
        {
            return await _dbContext.BankReconciliations
                .Include(x => x.Account)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.ReconciliationId == id && !x.IsDeleted)
                .Select(x => new ReconciliationDetailViewModel
                {
                    ReconciliationId = x.ReconciliationId,
                    ReconciliationNumber = x.ReconciliationNumber,
                    AccountId = x.AccountId,
                    AccountName = x.Account != null ? x.Account.AccountName : string.Empty,
                    StatementDate = x.StatementDate,
                    StatementEndingBalance = x.StatementEndingBalance,
                    GlEndingBalance = x.GlEndingBalance,
                    Status = x.Status,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count,
                    ClearedCount = x.Lines.Count(l => l.IsCleared),
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new ReconciliationLineViewModel
                    {
                        ReconciliationLineId = l.ReconciliationLineId,
                        ReconciliationId = l.ReconciliationId,
                        TransactionDate = l.TransactionDate,
                        DocumentNumber = l.DocumentNumber,
                        Description = l.Description,
                        Amount = l.Amount,
                        IsCleared = l.IsCleared
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ReconciliationLineViewModel>> GetBankStatementAsync(Guid accountId, DateTime asOfDate)
        {
            var endDate = asOfDate.Date;
            return await _dbContext.JournalLines
                .Include(x => x.JournalEntry)
                .Where(x => x.AccountId == accountId && !x.IsDeleted
                    && !x.JournalEntry.IsDeleted
                    && x.JournalEntry.Status == "Posted"
                    && x.JournalEntry.JournalDate <= endDate)
                .OrderBy(x => x.JournalEntry.JournalDate)
                .Select(x => new ReconciliationLineViewModel
                {
                    TransactionDate = x.JournalEntry.JournalDate,
                    DocumentNumber = x.JournalEntry.JournalNumber,
                    Description = x.JournalEntry.Description,
                    Amount = x.DebitAmount - x.CreditAmount,
                    IsCleared = true
                }).ToListAsync();
        }

        public async Task CreateReconciliationAsync(CreateReconciliationRequest request, Guid userId)
        {
            if (request.AccountId == Guid.Empty) throw new Exception("Bank account is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new Exception("At least one statement line is required");

            var account = await _dbContext.ChartOfAccounts.FirstOrDefaultAsync(x => x.AccountId == request.AccountId && !x.IsDeleted);
            if (account == null) throw new Exception("Bank account not found");

            var reconciliation = new BankReconciliationEntity
            {
                ReconciliationId = Guid.NewGuid(),
                ReconciliationNumber = await GenerateNumberAsync("RCN"),
                AccountId = request.AccountId,
                StatementDate = request.StatementDate,
                StatementEndingBalance = request.StatementEndingBalance,
                GlEndingBalance = request.GlEndingBalance,
                Status = "Draft",
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in request.Lines)
            {
                reconciliation.Lines.Add(new BankReconciliationLineEntity
                {
                    ReconciliationLineId = Guid.NewGuid(),
                    TransactionDate = line.TransactionDate,
                    DocumentNumber = line.DocumentNumber,
                    Description = line.Description,
                    Amount = line.Amount,
                    IsCleared = line.IsCleared,
                    CreatedBy = userId.ToString()
                });
            }

            _dbContext.BankReconciliations.Add(reconciliation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateReconciliationAsync(UpdateReconciliationRequest request, Guid userId)
        {
            var reconciliation = await _dbContext.BankReconciliations
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.ReconciliationId == request.ReconciliationId && !x.IsDeleted);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be edited");

            reconciliation.AccountId = request.AccountId;
            reconciliation.StatementDate = request.StatementDate;
            reconciliation.StatementEndingBalance = request.StatementEndingBalance;
            reconciliation.GlEndingBalance = request.GlEndingBalance;
            reconciliation.Notes = request.Notes;
            reconciliation.UpdatedAt = DateTime.UtcNow;
            reconciliation.UpdatedBy = userId.ToString();

            foreach (var existing in reconciliation.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

            foreach (var line in request.Lines)
            {
                reconciliation.Lines.Add(new BankReconciliationLineEntity
                {
                    ReconciliationLineId = Guid.NewGuid(),
                    TransactionDate = line.TransactionDate,
                    DocumentNumber = line.DocumentNumber,
                    Description = line.Description,
                    Amount = line.Amount,
                    IsCleared = line.IsCleared,
                    CreatedBy = userId.ToString()
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task PostReconciliationAsync(Guid id, Guid userId)
        {
            var reconciliation = await _dbContext.BankReconciliations
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.ReconciliationId == id && !x.IsDeleted);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status == "Posted") throw new Exception("Reconciliation is already posted");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be posted");

            var difference = reconciliation.StatementEndingBalance - reconciliation.GlEndingBalance;
            var clearedTotal = reconciliation.Lines.Where(l => l.IsCleared && !l.IsDeleted).Sum(l => l.Amount);
            var floatTotal = reconciliation.Lines.Where(l => !l.IsCleared && !l.IsDeleted).Sum(l => l.Amount);

            // Classic reconciliation identity: GL + cleared - float = statement
            if (reconciliation.GlEndingBalance + clearedTotal - floatTotal != reconciliation.StatementEndingBalance
                && Math.Abs((reconciliation.GlEndingBalance + clearedTotal - floatTotal) - reconciliation.StatementEndingBalance) > 0.01m)
            {
                throw new Exception($"Reconciliation does not balance. Current difference: {difference:C}. Adjust cleared/float flags on the statement lines.");
            }

            reconciliation.Status = "Posted";
            reconciliation.PostedDate = DateTime.UtcNow;
            reconciliation.PostedBy = userId;
            reconciliation.UpdatedAt = DateTime.UtcNow;
            reconciliation.UpdatedBy = userId.ToString();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteReconciliationAsync(Guid id, Guid userId)
        {
            var reconciliation = await _dbContext.BankReconciliations.FirstOrDefaultAsync(x => x.ReconciliationId == id && !x.IsDeleted);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be deleted");

            reconciliation.IsDeleted = true;
            reconciliation.DeletedAt = DateTime.UtcNow;
            reconciliation.DeletedBy = userId.ToString();
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Helpers

        private async Task<string> GenerateNumberAsync(string prefix)
        {
            string number;
            if (prefix == "TRF")
            {
                var last = await _dbContext.CashBankTransfers.Where(x => x.TransferNumber.StartsWith(prefix + "-"))
                    .OrderByDescending(x => x.TransferNumber).Select(x => x.TransferNumber).FirstOrDefaultAsync();
                var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[(prefix.Length + 1)..]) + 1;
                number = $"{prefix}-{next:D5}";
            }
            else
            {
                var last = await _dbContext.BankReconciliations.Where(x => x.ReconciliationNumber.StartsWith(prefix + "-"))
                    .OrderByDescending(x => x.ReconciliationNumber).Select(x => x.ReconciliationNumber).FirstOrDefaultAsync();
                var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[(prefix.Length + 1)..]) + 1;
                number = $"{prefix}-{next:D5}";
            }

            return number;
        }

        #endregion
    }
}

