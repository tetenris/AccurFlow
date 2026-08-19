using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Commands
{
    public record DeleteStockOpnameCommand(Guid Id, Guid UserId) : IRequest;
}