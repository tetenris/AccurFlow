using AccuFlow.Models.BaseModel;
using MediatR;

namespace AccuFlow.Application.Features.Employees.Queries
{
    public record GetEmployeeDatatableQuery(BaseDatatableRequest Request) : IRequest<BaseDatatableResponse>;
}