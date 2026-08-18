using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ProductionOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Production;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ProductionOrders.Handlers
{
    public class GetProductionOrderDatatableQueryHandler : IRequestHandler<GetProductionOrderDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<ProductionOrderEntity> _orderRepository;

        public GetProductionOrderDatatableQueryHandler(IRepository<ProductionOrderEntity> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetProductionOrderDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _orderRepository.Query()
                .Include(x => x.FinishedItem)
                .Include(x => x.Warehouse)
                .Include(x => x.Bom)
                .Include(x => x.JournalEntry)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.ProductionOrderNumber.ToLower().Contains(search)
                    || x.FinishedItem.ItemCode.ToLower().Contains(search)
                    || x.FinishedItem.ItemName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(x => x.ProductionDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new ProductionOrderViewModel
                {
                    ProductionOrderId = x.ProductionOrderId,
                    ProductionOrderNumber = x.ProductionOrderNumber,
                    BomNumber = x.Bom != null ? x.Bom.BomNumber : null,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    Quantity = x.Quantity,
                    ProductionDate = x.ProductionDate,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Status = x.Status,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}