using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockTransfers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockTransfer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockTransfers.Handlers
{
    public class GetStockTransferDatatableQueryHandler : IRequestHandler<GetStockTransferDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<StockTransferEntity> _stockTransferRepository;

        public GetStockTransferDatatableQueryHandler(IRepository<StockTransferEntity> stockTransferRepository)
        {
            _stockTransferRepository = stockTransferRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetStockTransferDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _stockTransferRepository.Query()
                .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse)
                .Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.WarehouseId.HasValue) query = query.Where(x => x.FromWarehouseId == r.WarehouseId || x.ToWarehouseId == r.WarehouseId);
            if (r.DateFrom.HasValue) query = query.Where(x => x.TransferDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.TransferDate <= r.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.StockTransferNumber.ToLower().Contains(search));
            }
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.TransferDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new StockTransferViewModel
                {
                    StockTransferId = x.StockTransferId,
                    StockTransferNumber = x.StockTransferNumber,
                    TransferDate = x.TransferDate,
                    FromWarehouseName = x.FromWarehouse.WarehouseName,
                    ToWarehouseName = x.ToWarehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalQuantity = x.Lines.Sum(l => l.Quantity)
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}