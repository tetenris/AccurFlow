using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.ChartOfAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class GetCoaDetailAccountsQueryHandler : IRequestHandler<GetCoaDetailAccountsQuery, List<ChartOfAccountViewModel>>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetCoaDetailAccountsQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<List<ChartOfAccountViewModel>> Handle(GetCoaDetailAccountsQuery request, CancellationToken cancellationToken)
        {
            return await _coaRepository.Query()
                .Where(x => !x.IsHeader && x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    IsActive = x.IsActive,
                    Level = x.Level
                })
                .ToListAsync(cancellationToken);
        }
    }
}