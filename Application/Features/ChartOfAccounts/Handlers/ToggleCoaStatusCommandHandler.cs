using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class ToggleCoaStatusCommandHandler : IRequestHandler<ToggleCoaStatusCommand>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleCoaStatusCommandHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ToggleCoaStatusCommand request, CancellationToken cancellationToken)
        {
            var account = await _coaRepository.FirstOrDefaultAsync(
                x => x.AccountId == request.AccountId && !x.IsDeleted,
                cancellationToken);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.IsActive = !account.IsActive;
            account.UpdatedBy = request.UserId.ToString();
            account.UpdatedAt = DateTime.UtcNow;

            _coaRepository.Update(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}