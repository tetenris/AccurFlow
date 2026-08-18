using AccuFlow.Models.FinancialStatement;
using MediatR;

namespace AccuFlow.Application.Features.FinancialStatements.Queries
{
    public record ExportBalanceSheetQuery(GetBalanceSheetRequest Request) : IRequest<byte[]>;
}