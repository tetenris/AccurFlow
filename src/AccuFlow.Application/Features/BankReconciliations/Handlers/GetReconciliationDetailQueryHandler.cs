using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class GetReconciliationDetailQueryHandler : IRequestHandler<GetReconciliationDetailQuery, ReconciliationDetailViewModel?>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;

        public GetReconciliationDetailQueryHandler(IRepository<BankReconciliationEntity> reconciliationRepository)
        {
            _reconciliationRepository = reconciliationRepository;
        }

        public async Task<ReconciliationDetailViewModel?> Handle(GetReconciliationDetailQuery request, CancellationToken cancellationToken)
        {
            return await _reconciliationRepository.Query()
                .Include(x => x.Account)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => x.ReconciliationId == request.ReconciliationId && !x.IsDeleted)
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
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}