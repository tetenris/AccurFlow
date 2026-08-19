using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class GetDeliveryByIdQueryHandler : IRequestHandler<GetDeliveryByIdQuery, DeliveryDetailViewModel?>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;

        public GetDeliveryByIdQueryHandler(IRepository<DeliveryOrderEntity> deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<DeliveryDetailViewModel?> Handle(GetDeliveryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.Query()
                .Include(x => x.SalesOrder).Include(x => x.Customer).Include(x => x.Invoice)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.DeliveryOrderId == request.Id && !x.IsDeleted)
                .Select(x => new DeliveryDetailViewModel
                {
                    DeliveryOrderId = x.DeliveryOrderId,
                    DeliveryNumber = x.DeliveryNumber,
                    DeliveryDate = x.DeliveryDate,
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.SalesOrder.OrderNumber,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    InvoiceNumber = x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    CanInvoice = x.Status == "Posted" && !x.InvoiceId.HasValue,
                    Lines = x.Lines.Select(l => new DeliveryLineViewModel
                    {
                        DeliveryOrderLineId = l.DeliveryOrderLineId,
                        SalesOrderLineId = l.SalesOrderLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}