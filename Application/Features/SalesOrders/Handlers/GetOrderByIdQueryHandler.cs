using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.SalesOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailViewModel?>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;

        public GetOrderByIdQueryHandler(IRepository<SalesOrderEntity> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDetailViewModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _orderRepository.Query()
                .Include(x => x.Customer).Include(x => x.Quotation)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.SalesOrderId == request.Id && !x.IsDeleted)
                .Select(x => new OrderDetailViewModel
                {
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.OrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.CustomerName,
                    QuotationId = x.QuotationId,
                    QuotationNumber = x.Quotation != null ? x.Quotation.QuotationNumber : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new OrderLineViewModel
                    {
                        SalesOrderLineId = l.SalesOrderLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountAmount = l.DiscountAmount,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}