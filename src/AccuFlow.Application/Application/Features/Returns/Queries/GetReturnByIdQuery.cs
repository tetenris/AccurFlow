using AccuFlow.Models.Return;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Queries
{
    public record GetReturnByIdQuery(Guid GoodsReturnId) : IRequest<ReturnDetailViewModel?>;
}