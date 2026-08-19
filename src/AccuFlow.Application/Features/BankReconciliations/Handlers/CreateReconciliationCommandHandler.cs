using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class CreateReconciliationCommandHandler : IRequestHandler<CreateReconciliationCommand>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReconciliationCommandHandler(
            IRepository<BankReconciliationEntity> reconciliationRepository,
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _reconciliationRepository = reconciliationRepository;
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateReconciliationCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            if (r.AccountId == Guid.Empty) throw new Exception("Bank account is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one statement line is required");

            var account = await _coaRepository.Query()
                .FirstOrDefaultAsync(x => x.AccountId == r.AccountId && !x.IsDeleted, cancellationToken);
            if (account == null) throw new Exception("Bank account not found");

            var reconciliation = new BankReconciliationEntity
            {
                ReconciliationId = Guid.NewGuid(),
                ReconciliationNumber = await GenerateNumberAsync(cancellationToken),
                AccountId = r.AccountId,
                StatementDate = r.StatementDate,
                StatementEndingBalance = r.StatementEndingBalance,
                GlEndingBalance = r.GlEndingBalance,
                Status = "Draft",
                Notes = r.Notes,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                reconciliation.Lines.Add(new BankReconciliationLineEntity
                {
                    ReconciliationLineId = Guid.NewGuid(),
                    TransactionDate = line.TransactionDate,
                    DocumentNumber = line.DocumentNumber,
                    Description = line.Description,
                    Amount = line.Amount,
                    IsCleared = line.IsCleared,
                    CreatedBy = userId.ToString()
                });
            }

            _reconciliationRepository.Add(reconciliation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _reconciliationRepository.Query()
                .Where(x => x.ReconciliationNumber.StartsWith("RCN-"))
                .OrderByDescending(x => x.ReconciliationNumber)
                .Select(x => x.ReconciliationNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"RCN-{next:D5}";
        }
    }
}