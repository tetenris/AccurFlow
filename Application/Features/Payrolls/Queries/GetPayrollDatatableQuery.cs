using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payroll;
using MediatR;

namespace AccuFlow.Application.Features.Payrolls.Queries
{
    public record GetPayrollDatatableQuery(BaseDatatableRequest Request) : IRequest<BaseDatatableResponse>;
}