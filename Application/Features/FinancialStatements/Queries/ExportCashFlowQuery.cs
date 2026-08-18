using AccuFlow.Models.FinancialStatement;
using MediatR;

namespace AccuFlow.Application.Features.FinancialStatements.Queries
{
    public record ExportCashFlowQuery(GetCashFlowStatementRequest Request) : IRequest<byte[]>;
}