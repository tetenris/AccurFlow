using MediatR;
using AccuFlow.Models.User;

namespace AccuFlow.Application.Features.Users.Queries
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserViewModel?>;
}