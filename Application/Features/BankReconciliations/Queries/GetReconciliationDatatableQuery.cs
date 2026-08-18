using AccuFlow.Models.BaseModel;
using AccuFlow.Models.CashBank;
using MediatR;

namespace AccuFlow.Application.Features.BankReconciliations.Queries
{
    public record GetReconciliationDatatableQuery(DataTableReconciliationRequest Request) : IRequest<BaseDatatableResponse>;
}