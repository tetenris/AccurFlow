using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockBatches.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockBatch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockBatches.Handlers
{
    public class GetStockBatchDatatableQueryHandler : IRequestHandler<GetStockBatchDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<StockBatchEntity> _stockBatchRepository;

        public GetStockBatchDatatableQueryHandler(IRepository<StockBatchEntity> stockBatchRepository)
        {
            _stockBatchRepository = stockBatchRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetStockBatchDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _stockBatchRepository.Query()
                .Include(x => x.Item)
                .Where(x => !x.IsDeleted);

            if (request.ItemId.HasValue) query = query.Where(x => x.ItemId == request.ItemId.Value);

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.BatchNumber.ToLower().Contains(search)
                    || x.Item.ItemCode.ToLower().Contains(search)
                    || x.Item.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new StockBatchViewModel
                {
                    StockBatchId = x.StockBatchId,
                    ItemId = x.ItemId,
                    ItemCode = x.Item.ItemCode,
                    ItemName = x.Item.ItemName,
                    BatchNumber = x.BatchNumber,
                    Quantity = x.Quantity,
                    RemainingQuantity = x.RemainingQuantity,
                    ExpiryDate = x.ExpiryDate,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    Status = x.RemainingQuantity <= 0 ? "Out" : (x.RemainingQuantity < x.Quantity ? "Partial" : "Available")
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}