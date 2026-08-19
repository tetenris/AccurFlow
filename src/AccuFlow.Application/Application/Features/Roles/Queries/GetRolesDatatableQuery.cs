using MediatR;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Role;

namespace AccuFlow.Application.Features.Roles.Queries
{
    public record GetRolesDatatableQuery(DataTableRoleRequest Request) : IRequest<BaseDatatableResponse>;
}