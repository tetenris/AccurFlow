using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.ChartOfAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class GetCoaParentAccountsQueryHandler : IRequestHandler<GetCoaParentAccountsQuery, List<ChartOfAccountViewModel>>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GetCoaParentAccountsQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<List<ChartOfAccountViewModel>> Handle(GetCoaParentAccountsQuery request, CancellationToken cancellationToken)
        {
            return await _coaRepository.Query()
                .Where(x => x.AccountType == request.AccountType && !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountViewModel
                {
                    AccountId = x.AccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    Level = x.Level
                })
                .ToListAsync(cancellationToken);
        }
    }
}