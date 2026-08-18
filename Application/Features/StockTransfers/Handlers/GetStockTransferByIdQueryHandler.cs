using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.StockTransfer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class GetStockTransferByIdQueryHandler : IRequestHandler<GetStockTransferByIdQuery, StockTransferDetailViewModel?>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;

        public GetStockTransferByIdQueryHandler(IRepository<StockTransferEntity> stockTransferRepository)
        {
            _stockTransferRepository = stockTransferRepository;
        }

        public async Task<StockTransferDetailViewModel?> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
        {
            return await _stockTransferRepository.Query()
                .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.StockTransferId == request.Id && !x.IsDeleted)
                .Select(x => new StockTransferDetailViewModel
                {
                    StockTransferId = x.StockTransferId,
                    StockTransferNumber = x.StockTransferNumber,
                    TransferDate = x.TransferDate,
                    FromWarehouseId = x.FromWarehouseId,
                    ToWarehouseId = x.ToWarehouseId,
                    FromWarehouseName = x.FromWarehouse.WarehouseName,
                    ToWarehouseName = x.ToWarehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalQuantity = x.Lines.Sum(l => l.Quantity),
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new StockTransferLineViewModel
                    {
                        StockTransferLineId = l.StockTransferLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item.ItemCode,
                        ItemName = l.Item.ItemName,
                        Quantity = l.Quantity
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}