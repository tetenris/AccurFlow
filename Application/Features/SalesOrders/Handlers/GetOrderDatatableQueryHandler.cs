using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.SalesOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class GetOrderDatatableQueryHandler : IRequestHandler<GetOrderDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;

        public GetOrderDatatableQueryHandler(IRepository<SalesOrderEntity> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetOrderDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _orderRepository.Query()
                .Include(x => x.Customer).Include(x => x.Quotation).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.OrderNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.OrderDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new OrderViewModel
                {
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.OrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    CustomerName = x.Customer.CustomerName,
                    QuotationNumber = x.Quotation != null ? x.Quotation.QuotationNumber : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}