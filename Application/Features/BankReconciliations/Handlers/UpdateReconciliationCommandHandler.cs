using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class UpdateReconciliationCommandHandler : IRequestHandler<UpdateReconciliationCommand>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateReconciliationCommandHandler(
            IRepository<BankReconciliationEntity> reconciliationRepository,
            IUnitOfWork unitOfWork)
        {
            _reconciliationRepository = reconciliationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateReconciliationCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            var reconciliation = await _reconciliationRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.ReconciliationId == r.ReconciliationId && !x.IsDeleted, cancellationToken);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be edited");

            reconciliation.AccountId = r.AccountId;
            reconciliation.StatementDate = r.StatementDate;
            reconciliation.StatementEndingBalance = r.StatementEndingBalance;
            reconciliation.GlEndingBalance = r.GlEndingBalance;
            reconciliation.Notes = r.Notes;
            reconciliation.UpdatedAt = DateTime.UtcNow;
            reconciliation.UpdatedBy = userId.ToString();

            foreach (var existing in reconciliation.Lines.Where(l => !l.IsDeleted).ToList())
            {
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = userId.ToString();
            }

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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}