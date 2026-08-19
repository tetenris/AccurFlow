using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class GetTransferDatatableQueryHandler : IRequestHandler<GetTransferDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;

        public GetTransferDatatableQueryHandler(IRepository<CashBankTransferEntity> transferRepository)
        {
            _transferRepository = transferRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetTransferDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _transferRepository.Query()
                .Include(x => x.FromAccount)
                .Include(x => x.ToAccount)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Status)) query = query.Where(x => x.Status == r.Status);
            if (r.DateFrom.HasValue) query = query.Where(x => x.TransferDate >= r.DateFrom.Value.Date);
            if (r.DateTo.HasValue) query = query.Where(x => x.TransferDate <= r.DateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.TransferNumber.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderByDescending(x => x.TransferDate)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new TransferViewModel
                {
                    TransferId = x.TransferId,
                    TransferNumber = x.TransferNumber,
                    TransferDate = x.TransferDate,
                    FromAccountName = x.FromAccount != null ? x.FromAccount.AccountName : string.Empty,
                    ToAccountName = x.ToAccount != null ? x.ToAccount.AccountName : string.Empty,
                    Amount = x.Amount,
                    Status = x.Status
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}