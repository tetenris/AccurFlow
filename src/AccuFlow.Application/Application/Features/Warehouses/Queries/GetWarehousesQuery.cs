using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Warehouses.Queries
{
    public record GetWarehousesQuery : IRequest<List<WarehouseEntity>>;
}