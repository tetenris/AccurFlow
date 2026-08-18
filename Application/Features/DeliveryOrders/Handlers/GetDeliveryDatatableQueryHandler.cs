using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Delivery;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class GetDeliveryDatatableQueryHandler : IRequestHandler<GetDeliveryDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;

        public GetDeliveryDatatableQueryHandler(IRepository<DeliveryOrderEntity> deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetDeliveryDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _deliveryRepository.Query()
                .Include(x => x.SalesOrder).Include(x => x.Customer).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.DeliveryNumber.ToLower().Contains(search) || x.SalesOrder.OrderNumber.ToLower().Contains(search) || x.Customer.CustomerName.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.DeliveryDate).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new DeliveryViewModel
                {
                    DeliveryOrderId = x.DeliveryOrderId,
                    DeliveryNumber = x.DeliveryNumber,
                    DeliveryDate = x.DeliveryDate,
                    SalesOrderId = x.SalesOrderId,
                    OrderNumber = x.SalesOrder.OrderNumber,
                    CustomerName = x.Customer.CustomerName,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}