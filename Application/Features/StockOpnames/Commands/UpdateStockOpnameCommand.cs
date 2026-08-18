using AccuFlow.Models.StockOpname;
using MediatR;

namespace AccuFlow.Application.Features.StockOpnames.Commands
{
    public record UpdateStockOpnameCommand(UpdateStockOpnameRequest Request, Guid UserId) : IRequest;
}