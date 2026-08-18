using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.StockOpname;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class GetStockOpnameDatatableQueryHandler : IRequestHandler<GetStockOpnameDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;

        public GetStockOpnameDatatableQueryHandler(IRepository<StockOpnameEntity> stockOpnameRepository)
        {
            _stockOpnameRepository = stockOpnameRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetStockOpnameDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _stockOpnameRepository.Query()
                .Include(x => x.Warehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.StockOpnameNumber.ToLower().Contains(search) || x.Warehouse.WarehouseName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.OpnameDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new StockOpnameViewModel
                {
                    StockOpnameId = x.StockOpnameId,
                    StockOpnameNumber = x.StockOpnameNumber,
                    OpnameDate = x.OpnameDate,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalDifference = x.Lines.Sum(l => l.DifferenceQuantity)
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}