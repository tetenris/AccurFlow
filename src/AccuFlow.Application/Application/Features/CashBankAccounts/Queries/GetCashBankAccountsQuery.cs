using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.CashBankAccounts.Queries
{
    public record GetCashBankAccountsQuery(int Usage) : IRequest<List<BankAccountViewModel>>;
}