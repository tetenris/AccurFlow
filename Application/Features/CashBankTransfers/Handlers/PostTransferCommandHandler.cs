using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Commands;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class PostTransferCommandHandler : IRequestHandler<PostTransferCommand>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostTransferCommandHandler(
            IRepository<CashBankTransferEntity> transferRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _transferRepository = transferRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostTransferCommand request, CancellationToken cancellationToken)
        {
            var transfer = await _transferRepository.Query()
                .FirstOrDefaultAsync(x => x.TransferId == request.TransferId && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status == "Posted") throw new Exception("Transfer is already posted");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be posted");

            var description = $"Auto journal for {transfer.TransferNumber}";
            var lines = new List<JournalLineRequest>
            {
                new JournalLineRequest
                {
                    AccountId = transfer.ToAccountId,
                    Description = description,
                    DebitAmount = transfer.Amount,
                    CreditAmount = 0
                },
                new JournalLineRequest
                {
                    AccountId = transfer.FromAccountId,
                    Description = description,
                    DebitAmount = 0,
                    CreditAmount = transfer.Amount
                }
            };

            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = transfer.TransferDate,
                Description = description,
                JournalLines = lines
            }, request.UserId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = transfer.TransferDate
            }, request.UserId), cancellationToken);

            transfer.JournalId = journalId;
            transfer.Status = "Posted";
            transfer.PostedDate = DateTime.UtcNow;
            transfer.PostedBy = request.UserId;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}