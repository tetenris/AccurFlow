using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePurchaseOrderCommandHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var po = new PurchaseOrderEntity
            {
                PurchaseOrderId = Guid.NewGuid(),
                PurchaseOrderNumber = await GenerateNumber(cancellationToken),
                OrderDate = r.OrderDate,
                ExpectedDate = r.ExpectedDate,
                SupplierId = r.SupplierId,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };
            foreach (var line in r.Lines)
            {
                var lineTotal = (line.Quantity * line.UnitPrice) + line.TaxAmount;
                po.Lines.Add(new PurchaseOrderLineEntity
                {
                    PurchaseOrderLineId = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    LineTotal = lineTotal,
                    CreatedBy = userId.ToString()
                });
                po.SubTotal += line.Quantity * line.UnitPrice;
                po.TaxAmount += line.TaxAmount;
                po.TotalAmount += lineTotal;
            }
            _poRepository.Add(po);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumber(CancellationToken cancellationToken)
        {
            var last = await _poRepository.Query()
                .Where(x => x.PurchaseOrderNumber.StartsWith("PO-"))
                .OrderByDescending(x => x.PurchaseOrderNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = last == null ? 1 : int.Parse(last.PurchaseOrderNumber[3..]) + 1;
            return $"PO-{next:D5}";
        }
    }
}