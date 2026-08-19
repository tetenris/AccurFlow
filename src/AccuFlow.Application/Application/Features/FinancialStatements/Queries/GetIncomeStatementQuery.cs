using AccuFlow.Models.FinancialStatement;
using MediatR;

namespace AccuFlow.Application.Features.FinancialStatements.Queries
{
    public record GetIncomeStatementQuery(GetIncomeStatementRequest Request) : IRequest<IncomeStatementViewModel>;
}