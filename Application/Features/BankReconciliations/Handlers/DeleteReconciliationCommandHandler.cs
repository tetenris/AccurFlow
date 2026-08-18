using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BankReconciliations.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BankReconciliations.Handlers
{
    public class DeleteReconciliationCommandHandler : IRequestHandler<DeleteReconciliationCommand>
    {
        private readonly IRepository<BankReconciliationEntity> _reconciliationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReconciliationCommandHandler(
            IRepository<BankReconciliationEntity> reconciliationRepository,
            IUnitOfWork unitOfWork)
        {
            _reconciliationRepository = reconciliationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteReconciliationCommand request, CancellationToken cancellationToken)
        {
            var reconciliation = await _reconciliationRepository.Query()
                .FirstOrDefaultAsync(x => x.ReconciliationId == request.ReconciliationId && !x.IsDeleted, cancellationToken);
            if (reconciliation == null) throw new Exception("Reconciliation not found");
            if (reconciliation.Status != "Draft") throw new Exception("Only draft reconciliations can be deleted");

            reconciliation.IsDeleted = true;
            reconciliation.DeletedAt = DateTime.UtcNow;
            reconciliation.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}