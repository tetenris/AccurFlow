using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class PostReconciliationCommandHandler : IRequestHandler<PostReconciliationCommand>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PostReconciliationCommandHandler(
            IRepository<BankReconciliationEntity> reconciliationRepository,
            IUnitOfWork unitOfWork)
        {
            _reconciliationRepository = reconciliationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PostReconciliationCommand request, CancellationToken cancellationToken)
        {
            var reconciliation = await _reconciliationRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.ReconciliationId == request.ReconciliationId && !x.IsDeleted, cancellationToken);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status == "Posted") throw new Exception("Reconciliation is already posted");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be posted");

            var difference = reconciliation.StatementEndingBalance - reconciliation.GlEndingBalance;
            var clearedTotal = reconciliation.Lines.Where(l => l.IsCleared && !l.IsDeleted).Sum(l => l.Amount);
            var floatTotal = reconciliation.Lines.Where(l => !l.IsCleared && !l.IsDeleted).Sum(l => l.Amount);

            // Classic reconciliation identity: GL + cleared - float = statement
            if (reconciliation.GlEndingBalance + clearedTotal - floatTotal != reconciliation.StatementEndingBalance
                && Math.Abs((reconciliation.GlEndingBalance + clearedTotal - floatTotal) - reconciliation.StatementEndingBalance) > 0.01m)
            {
                throw new Exception($"Reconciliation does not balance. Current difference: {difference:C}. Adjust cleared/float flags on the statement lines.");
            }

            reconciliation.Status = "Posted";
            reconciliation.PostedDate = DateTime.UtcNow;
            reconciliation.PostedBy = request.UserId;
            reconciliation.UpdatedAt = DateTime.UtcNow;
            reconciliation.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}