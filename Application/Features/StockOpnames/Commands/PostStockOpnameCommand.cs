using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Commands
{
    public record PostStockOpnameCommand(Guid Id, Guid UserId) : IRequest;
}