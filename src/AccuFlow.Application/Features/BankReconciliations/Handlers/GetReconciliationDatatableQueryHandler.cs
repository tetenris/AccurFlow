using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class GetReconciliationDatatableQueryHandler : IRequestHandler<GetReconciliationDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;

        public GetReconciliationDatatableQueryHandler(IRepository<BankReconciliationEntity> reconciliationRepository)
        {
            _reconciliationRepository = reconciliationRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetReconciliationDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _reconciliationRepository.Query()
                .Include(x => x.Account)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.ReconciliationNumber.ToLower().Contains(search) || x.Account != null && x.Account.AccountName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.StatementDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
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
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}