using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.CashBank;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class GetTransferDetailQueryHandler : IRequestHandler<GetTransferDetailQuery, TransferDetailViewModel?>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;

        public GetTransferDetailQueryHandler(IRepository<CashBankTransferEntity> transferRepository)
        {
            _transferRepository = transferRepository;
        }

        public async Task<TransferDetailViewModel?> Handle(GetTransferDetailQuery request, CancellationToken cancellationToken)
        {
            return await _transferRepository.Query()
                .Include(x => x.FromAccount)
                .Include(x => x.ToAccount)
                .Include(x => x.JournalEntry)
                .Where(x => x.TransferId == request.TransferId && !x.IsDeleted)
                .Select(x => new TransferDetailViewModel
                {
                    TransferId = x.TransferId,
                    TransferNumber = x.TransferNumber,
                    TransferDate = x.TransferDate,
                    FromAccountId = x.FromAccountId,
                    ToAccountId = x.ToAccountId,
                    FromAccountName = x.FromAccount != null ? x.FromAccount.AccountName : string.Empty,
                    ToAccountName = x.ToAccount != null ? x.ToAccount.AccountName : string.Empty,
                    Amount = x.Amount,
                    Description = x.Description,
                    ReferenceNumber = x.ReferenceNumber,
                    Status = x.Status,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    CanEdit = x.Status == "Draft",
                    CanDelete = x.Status == "Draft",
                    CanPost = x.Status == "Draft"
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}