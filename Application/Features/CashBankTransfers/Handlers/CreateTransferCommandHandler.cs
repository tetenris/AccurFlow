using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.CashBankTransfers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.CashBankTransfers.Handlers
{
    public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand>
    {
        private readonly IRepository<CashBankTransferEntity> _transferRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTransferCommandHandler(
            IRepository<CashBankTransferEntity> transferRepository,
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _transferRepository = transferRepository;
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateTransferCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            ValidateTransfer(r.FromAccountId, r.ToAccountId, r.Amount);
            await ValidateTransferAccountsAsync(r.FromAccountId, r.ToAccountId, cancellationToken);

            var transfer = new CashBankTransferEntity
            {
                TransferId = Guid.NewGuid(),
                TransferNumber = await GenerateNumberAsync(cancellationToken),
                TransferDate = r.TransferDate,
                FromAccountId = r.FromAccountId,
                ToAccountId = r.ToAccountId,
                Amount = r.Amount,
                Description = r.Description,
                ReferenceNumber = r.ReferenceNumber,
                Status = "Draft",
                CreatedBy = userId.ToString()
            };

            _transferRepository.Add(transfer);
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

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _transferRepository.Query()
                .Where(x => x.TransferNumber.StartsWith("TRF-"))
                .OrderByDescending(x => x.TransferNumber)
                .Select(x => x.TransferNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"TRF-{next:D5}";
        }
    }
}