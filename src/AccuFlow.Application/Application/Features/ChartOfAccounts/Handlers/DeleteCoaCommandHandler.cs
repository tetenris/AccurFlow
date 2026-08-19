using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class DeleteCoaCommandHandler : IRequestHandler<DeleteCoaCommand>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCoaCommandHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteCoaCommand request, CancellationToken cancellationToken)
        {
            var account = await _coaRepository.FirstOrDefaultAsync(
                x => x.AccountId == request.AccountId && !x.IsDeleted,
                cancellationToken);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.IsActive)
            {
                throw new Exception("Cannot delete active account. Please deactivate the account first.");
            }

            if (await HasTransactionsAsync(request.AccountId, cancellationToken))
            {
                throw new Exception("Cannot delete account that has been used in transactions. Consider deactivating instead.");
            }

            if (await HasChildrenAsync(request.AccountId, cancellationToken))
            {
                throw new Exception("Cannot delete account that has sub-accounts. Delete or move sub-accounts first.");
            }

            account.IsDeleted = true;
            account.DeletedBy = request.UserId.ToString();
            account.DeletedAt = DateTime.UtcNow;
            _coaRepository.Update(account);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<bool> HasTransactionsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return false;
        }

        private async Task<bool> HasChildrenAsync(Guid accountId, CancellationToken cancellationToken)
        {
            return await _coaRepository.AnyAsync(
                x => x.ParentAccountId == accountId && !x.IsDeleted,
                cancellationToken);
        }
    }
}