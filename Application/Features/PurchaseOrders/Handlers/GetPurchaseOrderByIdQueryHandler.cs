using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.PurchaseOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailViewModel?>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;

        public GetPurchaseOrderByIdQueryHandler(IRepository<PurchaseOrderEntity> poRepository)
        {
            _poRepository = poRepository;
        }

        public async Task<PurchaseOrderDetailViewModel?> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _poRepository.Query()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseInvoice)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.PurchaseOrderId == request.Id && !x.IsDeleted)
                .Select(x => new PurchaseOrderDetailViewModel
                {
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrderNumber,
                    OrderDate = x.OrderDate,
                    ExpectedDate = x.ExpectedDate,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.SupplierName,
                    Status = x.Status,
                    TotalAmount = x.TotalAmount,
                    PurchaseInvoiceId = x.PurchaseInvoiceId,
                    PurchaseInvoiceNumber = x.PurchaseInvoice != null ? x.PurchaseInvoice.InvoiceNumber : null,
                    Notes = x.Notes,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    CanConvert = x.Status == "Approved" && !x.PurchaseInvoiceId.HasValue,
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new PurchaseOrderLineViewModel
                    {
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
                        ItemId = l.ItemId,
                        ItemName = l.Item != null ? l.Item.ItemName : null,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}