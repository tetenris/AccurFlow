using AccuFlow.Models.StockTransfer;
using MediatR;

namespace AccuFlow.Application.Features.StockTransfers.Queries
{
    public record GetStockTransferByIdQuery(Guid Id) : IRequest<StockTransferDetailViewModel?>;
}