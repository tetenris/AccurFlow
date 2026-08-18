using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ProductionOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Production;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ProductionOrders.Handlers
{
    public class GetProductionOrderByIdQueryHandler : IRequestHandler<GetProductionOrderByIdQuery, ProductionOrderDetailViewModel?>
    {
        private readonly IRepository<ProductionOrderEntity> _orderRepository;

        public GetProductionOrderByIdQueryHandler(IRepository<ProductionOrderEntity> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ProductionOrderDetailViewModel?> Handle(GetProductionOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _orderRepository.Query()
                .Include(x => x.FinishedItem)
                .Include(x => x.Warehouse)
                .Include(x => x.Bom)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.ComponentItem)
                .Where(x => x.ProductionOrderId == request.ProductionOrderId && !x.IsDeleted)
                .Select(x => new ProductionOrderDetailViewModel
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
                    Lines = x.Lines.OrderBy(l => l.ComponentItem.ItemCode).Select(l => new ProductionOrderLineViewModel
                    {
                        ProductionOrderLineId = l.ProductionOrderLineId,
                        ComponentItemId = l.ComponentItemId,
                        ComponentItemCode = l.ComponentItem.ItemCode,
                        ComponentItemName = l.ComponentItem.ItemName,
                        QuantityRequired = l.QuantityRequired,
                        UnitCost = l.UnitCost
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}