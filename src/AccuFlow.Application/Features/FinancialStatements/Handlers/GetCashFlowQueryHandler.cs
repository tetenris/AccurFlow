using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.FinancialStatements.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.FinancialStatement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.FinancialStatements.Handlers
{
    public class GetCashFlowQueryHandler : IRequestHandler<GetCashFlowQuery, CashFlowStatementViewModel>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IRepository<JournalLineEntity> _journalLineRepository;

        public GetCashFlowQueryHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IRepository<JournalLineEntity> journalLineRepository)
        {
            _coaRepository = coaRepository;
            _journalLineRepository = journalLineRepository;
        }

        public async Task<CashFlowStatementViewModel> Handle(GetCashFlowQuery request, CancellationToken cancellationToken)
        {
            var cashAccounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountName.Contains("Cash") || a.AccountName.Contains("Bank") || a.AccountCode.StartsWith("1-1")))
                .ToListAsync(cancellationToken);

            decimal beginningBalance = 0;
            decimal endingBalance = 0;

            foreach (var account in cashAccounts)
            {
                beginningBalance += await FinancialStatementHelper.GetOpeningBalanceAsync(
                    _journalLineRepository, account.AccountId, request.Request.DateFrom, cancellationToken);
                endingBalance += await FinancialStatementHelper.GetAccountBalanceAsync(
                    _journalLineRepository, account.AccountId, request.Request.DateTo, cancellationToken);
            }

            var sections = new List<CashFlowSectionViewModel>();

            var operatingAccounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Revenue" || a.AccountType == "Expense"))
                .ToListAsync(cancellationToken);

            var operatingLines = new List<CashFlowLineViewModel>();
            foreach (var account in operatingAccounts)
            {
                var balance = await FinancialStatementHelper.GetAccountBalanceAsync(
                    _journalLineRepository, account.AccountId, request.Request.DateTo, cancellationToken);
                if (balance != 0)
                {
                    operatingLines.Add(new CashFlowLineViewModel
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        Amount = account.AccountType == "Revenue" ? balance : -balance
                    });
                }
            }

            if (operatingLines.Any())
            {
                sections.Add(new CashFlowSectionViewModel
                {
                    SectionName = "Operating Activities",
                    Lines = operatingLines,
                    Subtotal = operatingLines.Sum(l => l.Amount)
                });
            }

            var investingAccounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && a.AccountType == "Asset"
                    && !a.AccountName.Contains("Cash") && !a.AccountName.Contains("Bank"))
                .ToListAsync(cancellationToken);

            var investingLines = new List<CashFlowLineViewModel>();
            foreach (var account in investingAccounts)
            {
                var balance = await FinancialStatementHelper.GetAccountBalanceAsync(
                    _journalLineRepository, account.AccountId, request.Request.DateTo, cancellationToken);
                if (balance != 0)
                {
                    investingLines.Add(new CashFlowLineViewModel
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        Amount = -balance
                    });
                }
            }

            if (investingLines.Any())
            {
                sections.Add(new CashFlowSectionViewModel
                {
                    SectionName = "Investing Activities",
                    Lines = investingLines,
                    Subtotal = investingLines.Sum(l => l.Amount)
                });
            }

            var financingAccounts = await _coaRepository.Query()
                .Where(a => !a.IsDeleted && a.IsActive && !a.IsHeader
                    && (a.AccountType == "Liability" || a.AccountType == "Equity"))
                .ToListAsync(cancellationToken);

            var financingLines = new List<CashFlowLineViewModel>();
            foreach (var account in financingAccounts)
            {
                var balance = await FinancialStatementHelper.GetAccountBalanceAsync(
                    _journalLineRepository, account.AccountId, request.Request.DateTo, cancellationToken);
                if (balance != 0)
                {
                    financingLines.Add(new CashFlowLineViewModel
                    {
                        AccountId = account.AccountId,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        Amount = balance
                    });
                }
            }

            if (financingLines.Any())
            {
                sections.Add(new CashFlowSectionViewModel
                {
                    SectionName = "Financing Activities",
                    Lines = financingLines,
                    Subtotal = financingLines.Sum(l => l.Amount)
                });
            }

            var netOperating = sections.FirstOrDefault(s => s.SectionName == "Operating Activities")?.Subtotal ?? 0;
            var netInvesting = sections.FirstOrDefault(s => s.SectionName == "Investing Activities")?.Subtotal ?? 0;
            var netFinancing = sections.FirstOrDefault(s => s.SectionName == "Financing Activities")?.Subtotal ?? 0;
            var netChange = endingBalance - beginningBalance;

            return new CashFlowStatementViewModel
            {
                DateFrom = request.Request.DateFrom,
                DateTo = request.Request.DateTo,
                BeginningCashBalance = beginningBalance,
                Sections = sections,
                NetCashFromOperating = netOperating,
                NetCashFromInvesting = netInvesting,
                NetCashFromFinancing = netFinancing,
                NetIncreaseDecrease = netChange,
                EndingCashBalance = endingBalance
            };
        }
    }
}