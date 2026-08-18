using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Return;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class GetReturnByIdQueryHandler : IRequestHandler<GetReturnByIdQuery, ReturnDetailViewModel?>
    {
        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;

        public GetReturnByIdQueryHandler(IRepository<GoodsReturnEntity> goodsReturnRepository)
        {
            _goodsReturnRepository = goodsReturnRepository;
        }

        public async Task<ReturnDetailViewModel?> Handle(GetReturnByIdQuery request, CancellationToken cancellationToken)
        {
            return await _goodsReturnRepository.Query()
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Include(x => x.JournalEntry)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(x => x.Item)
                .Where(x => x.GoodsReturnId == request.GoodsReturnId && !x.IsDeleted)
                .Select(x => new ReturnDetailViewModel
                {
                    GoodsReturnId = x.GoodsReturnId,
                    GoodsReturnNumber = x.GoodsReturnNumber,
                    ReturnType = x.ReturnType,
                    ReturnDate = x.ReturnDate,
                    InvoiceNumber = x.Invoice.InvoiceNumber,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount,
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.Warehouse.WarehouseName,
                    Notes = x.Notes,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft",
                    Lines = x.Lines.Select(l => new ReturnLineViewModel
                    {
                        GoodsReturnLineId = l.GoodsReturnLineId,
                        InvoiceLineId = l.InvoiceLineId,
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