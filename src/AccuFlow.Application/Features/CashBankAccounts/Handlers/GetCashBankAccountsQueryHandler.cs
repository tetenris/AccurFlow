using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankAccounts.Queries;
using AccuFlow.Application.Features.GeneralLedgers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankAccounts.Handlers
{
    public class GetCashBankAccountsQueryHandler : IRequestHandler<GetCashBankAccountsQuery, List<BankAccountViewModel>>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly ISender _mediator;

        public GetCashBankAccountsQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            ISender mediator)
        {
            _coaRepository = coaRepository;
            _mediator = mediator;
        }

        public async Task<List<BankAccountViewModel>> Handle(GetCashBankAccountsQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _coaRepository.Query()
                .AsNoTracking()
                .Where(x => x.AccountUsage == request.Usage && !x.IsHeader && x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.AccountCode)
                .ToListAsync(cancellationToken);

            var result = new List<BankAccountViewModel>();
            foreach (var account in accounts)
            {
                var balance = await _mediator.Send(new GetAccountBalanceQuery(account.AccountId), cancellationToken);
                result.Add(new BankAccountViewModel
                {
                    AccountId = account.AccountId,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Usage = account.AccountUsage,
                    Balance = balance
                });
            }

            return result;
        }
    }
}