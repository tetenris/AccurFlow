using MediatR;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.User;

namespace AccuFlow.Application.Features.Users.Queries
{
    public record GetUsersDatatableQuery(DataTableUserRequest Request) : IRequest<BaseDatatableResponse>;
}