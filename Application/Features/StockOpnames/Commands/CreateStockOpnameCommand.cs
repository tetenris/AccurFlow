using AccuFlow.Models.StockOpname;
using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Commands
{
    public record CreateStockOpnameCommand(CreateStockOpnameRequest Request, Guid UserId) : IRequest;
}