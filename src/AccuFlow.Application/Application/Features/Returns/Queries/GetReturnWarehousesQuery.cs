using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Returns.Queries
{
    public record GetReturnWarehousesQuery : IRequest<List<WarehouseEntity>>;
}