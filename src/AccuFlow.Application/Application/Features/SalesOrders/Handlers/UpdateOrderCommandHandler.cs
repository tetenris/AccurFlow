using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.SalesOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderCommandHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var order = await _orderRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.SalesOrderId == r.SalesOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be edited");

            order.OrderDate = r.OrderDate;
            order.ExpectedDate = r.ExpectedDate;
            order.CustomerId = r.CustomerId;
            order.QuotationId = r.QuotationId;
            order.Notes = r.Notes;
            foreach (var existing in order.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true; existing.DeletedAt = DateTime.UtcNow; existing.DeletedBy = userId.ToString();
            }
            order.SubTotal = 0; order.DiscountAmount = 0; order.TaxAmount = 0; order.TotalAmount = 0;
            FillTotals(order, r.Lines, userId);
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = userId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private void FillTotals(SalesOrderEntity order, IEnumerable<OrderLineRequest> lines, Guid userId)
        {
            foreach (var line in lines)
            {
                if (line.Quantity <= 0) continue;
                var lineTotal = (line.Quantity * line.UnitPrice) - line.DiscountAmount + line.TaxAmount;
                order.Lines.Add(new SalesOrderLineEntity
                {
                    SalesOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                order.SubTotal += line.Quantity * line.UnitPrice;
                order.DiscountAmount += line.DiscountAmount;
                order.TaxAmount += line.TaxAmount;
                order.TotalAmount += lineTotal;
            }
        }
    }
}