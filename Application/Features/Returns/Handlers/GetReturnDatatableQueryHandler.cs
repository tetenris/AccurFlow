using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Return;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class GetReturnDatatableQueryHandler : IRequestHandler<GetReturnDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;

        public GetReturnDatatableQueryHandler(IRepository<GoodsReturnEntity> goodsReturnRepository)
        {
            _goodsReturnRepository = goodsReturnRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetReturnDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _goodsReturnRepository.Query()
                .Include(x => x.Invoice)
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.ReturnType)) query = query.Where(x => x.ReturnType == r.ReturnType);
            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.GoodsReturnNumber.ToLower().Contains(search) || x.Invoice.InvoiceNumber.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.ReturnDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new ReturnViewModel
                {
                    GoodsReturnId = x.GoodsReturnId,
                    GoodsReturnNumber = x.GoodsReturnNumber,
                    ReturnType = x.ReturnType,
                    ReturnDate = x.ReturnDate,
                    InvoiceNumber = x.Invoice.InvoiceNumber,
                    PartnerName = x.Customer != null ? x.Customer.CustomerName : x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    Status = x.Status,
                    LineCount = x.Lines.Count,
                    TotalAmount = x.TotalAmount
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}