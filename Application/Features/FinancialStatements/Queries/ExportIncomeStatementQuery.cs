using AccuFlow.Models.FinancialStatement;
using MediatR;

namespace AccuFlow.Application.Features.FinancialStatements.Queries
{
    public record ExportIncomeStatementQuery(GetIncomeStatementRequest Request) : IRequest<byte[]>;
}