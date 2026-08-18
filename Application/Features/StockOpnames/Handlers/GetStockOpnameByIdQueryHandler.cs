using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockOpnames.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.StockOpname;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockOpnames.Handlers
{
    public class GetStockOpnameByIdQueryHandler : IRequestHandler<GetStockOpnameByIdQuery, StockOpnameDetailViewModel?>
    {
        private readonly IRepository<StockOpnameEntity> _stockOpnameRepository;

        public GetStockOpnameByIdQueryHandler(IRepository<StockOpnameEntity> stockOpnameRepository)
        {
            _stockOpnameRepository = stockOpnameRepository;
        }

        public async Task<StockOpnameDetailViewModel?> Handle(GetStockOpnameByIdQuery request, CancellationToken cancellationToken)
        {
            return await _stockOpnameRepository.Query()
                .Include(x => x.Warehouse)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.StockOpnameId == request.Id && !x.IsDeleted)
                .Select(x => new StockOpnameDetailViewModel
                {
                    StockOpnameId = x.StockOpnameId,
                    StockOpnameNumber = x.StockOpnameNumber,
                    OpnameDate = x.OpnameDate,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count,
                    TotalDifference = x.Lines.Sum(l => l.DifferenceQuantity),
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new StockOpnameLineViewModel
                    {
                        ItemId = l.ItemId,
                        ItemCode = l.Item.ItemCode,
                        ItemName = l.Item.ItemName,
                        Unit = l.Item.Unit,
                        SystemQuantity = l.SystemQuantity,
                        ActualQuantity = l.ActualQuantity,
                        DifferenceQuantity = l.DifferenceQuantity,
                        UnitCost = l.Item.PurchasePrice,
                        Notes = l.Notes
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}