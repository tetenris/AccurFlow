using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.ChartOfAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class GetCoaDatatableQueryHandler : IRequestHandler<GetCoaDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetCoaDatatableQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetCoaDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            IQueryable<ChartOfAccountEntity> query = _coaRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount);

            var totalRecord = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x =>
                    x.AccountCode.ToLower().Contains(search) ||
                    x.AccountName.ToLower().Contains(search) ||
                    x.AccountType.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrEmpty(r.AccountType))
            {
                query = query.Where(x => x.AccountType == r.AccountType);
            }

            if (r.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == r.IsActive.Value);
            }

            if (r.IsHeader.HasValue)
            {
                query = query.Where(x => x.IsHeader == r.IsHeader.Value);
            }

            if (r.ParentAccountId.HasValue)
            {
                query = query.Where(x => x.ParentAccountId == r.ParentAccountId.Value);
            }

            if (!string.IsNullOrEmpty(r.OrderBy))
            {
                query = query.OrderBy($"{r.OrderBy} {r.OrderType}");
            }
            else
            {
                query = query.OrderBy(x => x.AccountCode);
            }

            var totalFiltered = await query.CountAsync(cancellationToken);

            var data = await query
                .Skip((r.Page - 1) * r.Size)
                .Take(r.Size)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Description = x.Description,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountName = x.ParentAccount != null ? x.ParentAccount.AccountName : null,
                    IsHeader = x.IsHeader,
                    IsActive = x.IsActive,
                    OpeningBalance = x.OpeningBalance,
                    NormalBalance = x.NormalBalance,
                    Currency = x.Currency,
                    Level = x.Level,
                    HasChildren = x.ChildAccounts.Any(c => !c.IsDeleted),
                    ChildCount = x.ChildAccounts.Count(c => !c.IsDeleted),
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedAt = x.UpdatedAt,
                    UpdatedBy = x.UpdatedBy
                })
                .ToListAsync(cancellationToken);

            return new BaseDatatableResponse
            {
                Draw = r.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }
    }
}