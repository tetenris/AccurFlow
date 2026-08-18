using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.SalesOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            if (r.CustomerId == Guid.Empty) throw new Exception("Customer is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one item line is required");

            var order = new SalesOrderEntity
            {
                SalesOrderId = Guid.NewGuid(),
                OrderNumber = await GenerateNumberAsync(cancellationToken),
                OrderDate = r.OrderDate,
                ExpectedDate = r.ExpectedDate,
                CustomerId = r.CustomerId,
                QuotationId = r.QuotationId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            FillTotals(order, r.Lines, userId);
            _orderRepository.Add(order);
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

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _orderRepository.Query()
                .Where(x => x.OrderNumber.StartsWith("SO-"))
                .OrderByDescending(x => x.OrderNumber)
                .Select(x => x.OrderNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[3..]) + 1;
            return $"SO-{next:D5}";
        }
    }
}