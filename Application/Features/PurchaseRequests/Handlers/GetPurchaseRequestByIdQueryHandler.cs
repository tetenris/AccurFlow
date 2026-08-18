using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseRequests.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.PurchaseRequest;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseRequests.Handlers
{
    public class GetPurchaseRequestByIdQueryHandler : IRequestHandler<GetPurchaseRequestByIdQuery, PurchaseRequestDetailViewModel?>
    {
        private readonly IRepository<PurchaseRequestEntity> _prRepository;

        public GetPurchaseRequestByIdQueryHandler(IRepository<PurchaseRequestEntity> prRepository)
        {
            _prRepository = prRepository;
        }

        public async Task<PurchaseRequestDetailViewModel?> Handle(GetPurchaseRequestByIdQuery request, CancellationToken cancellationToken)
        {
            return await _prRepository.Query()
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.PurchaseRequestId == request.Id && !x.IsDeleted)
                .Select(x => new PurchaseRequestDetailViewModel
                {
                    PurchaseRequestId = x.PurchaseRequestId,
                    PurchaseRequestNumber = x.PurchaseRequestNumber,
                    RequestDate = x.RequestDate,
                    RequiredDate = x.RequiredDate,
                    RequestedBy = x.RequestedBy,
                    Department = x.Department,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    Notes = x.Notes,
                    PurchaseOrderId = x.PurchaseOrderId,
                    PurchaseOrderNumber = x.PurchaseOrder != null ? x.PurchaseOrder.PurchaseOrderNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanApprove = x.Status == "Draft",
                    CanConvert = x.Status == "Approved" && !x.PurchaseOrderId.HasValue,
                    Lines = x.Lines.Where(l => !l.IsDeleted).Select(l => new PurchaseRequestLineViewModel
                    {
                        PurchaseRequestLineId = l.PurchaseRequestLineId,
                        ItemId = l.ItemId,
                        ItemCode = l.Item != null ? l.Item.ItemCode : string.Empty,
                        ItemName = l.Item != null ? l.Item.ItemName : string.Empty,
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