using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.ChartOfAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class GetCoaHierarchyQueryHandler : IRequestHandler<GetCoaHierarchyQuery, List<ChartOfAccountViewModel>>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetCoaHierarchyQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<List<ChartOfAccountViewModel>> Handle(GetCoaHierarchyQuery request, CancellationToken cancellationToken)
        {
            return await _coaRepository.Query()
                .Where(x => !x.IsDeleted)
                .Include(x => x.ParentAccount)
                .OrderBy(x => x.AccountCode)
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
                    CreatedBy = x.CreatedBy
                })
                .ToListAsync(cancellationToken);
        }
    }
}