using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class UpdateTransferCommandHandler : IRequestHandler<UpdateTransferCommand>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTransferCommandHandler(
            IRepository<CashBankTransferEntity> transferRepository,
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _transferRepository = transferRepository;
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateTransferCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            var transfer = await _transferRepository.Query()
                .FirstOrDefaultAsync(x => x.TransferId == r.TransferId && !x.IsDeleted, cancellationToken);
            if (transfer == null) throw new Exception("Transfer not found");
            if (transfer.Status != "Draft") throw new Exception("Only draft transfers can be edited");

            ValidateTransfer(r.FromAccountId, r.ToAccountId, r.Amount);
            await ValidateTransferAccountsAsync(r.FromAccountId, r.ToAccountId, cancellationToken);

            transfer.FromAccountId = r.FromAccountId;
            transfer.ToAccountId = r.ToAccountId;
            transfer.TransferDate = r.TransferDate;
            transfer.Amount = r.Amount;
            transfer.Description = r.Description;
            transfer.ReferenceNumber = r.ReferenceNumber;
            transfer.UpdatedAt = DateTime.UtcNow;
            transfer.UpdatedBy = userId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidateTransferAccountsAsync(Guid fromAccountId, Guid toAccountId, CancellationToken cancellationToken)
        {
            var accountIds = new[] { fromAccountId, toAccountId };
            var valid = await _coaRepository.Query()
                .CountAsync(x => accountIds.Contains(x.AccountId) && (x.AccountUsage == 1 || x.AccountUsage == 2) && !x.IsHeader && x.IsActive && !x.IsDeleted, cancellationToken);
            if (valid != 2) throw new Exception("Both accounts must be active cash/bank accounts");
        }

        private void ValidateTransfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            if (fromAccountId == toAccountId) throw new Exception("From and To accounts must be different");
            if (amount <= 0) throw new Exception("Amount must be greater than zero");
        }
    }
}