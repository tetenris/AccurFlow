using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class UpdateCoaCommandHandler : IRequestHandler<UpdateCoaCommand>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCoaCommandHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateCoaCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var account = await _coaRepository.FirstOrDefaultAsync(
                x => x.AccountId == r.AccountId && !x.IsDeleted,
                cancellationToken);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var codeExists = await _coaRepository.AnyAsync(
                x => x.AccountCode == r.AccountCode && x.AccountId != r.AccountId && !x.IsDeleted,
                cancellationToken);
            if (codeExists)
            {
                throw new Exception($"Account code '{r.AccountCode}' already exists");
            }

            var oldParentId = account.ParentAccountId;

            if (r.ParentAccountId != oldParentId)
            {
                if (r.ParentAccountId.HasValue)
                {
                    if (await IsCircularReferenceAsync(r.AccountId, r.ParentAccountId.Value, cancellationToken))
                    {
                        throw new Exception("Cannot set descendant account as parent (circular reference)");
                    }

                    var parentAccount = await _coaRepository.FirstOrDefaultAsync(
                        x => x.AccountId == r.ParentAccountId.Value && !x.IsDeleted,
                        cancellationToken);

                    if (parentAccount == null)
                    {
                        throw new Exception("Parent account not found");
                    }

                    if (parentAccount.AccountType != r.AccountType)
                    {
                        throw new Exception("Sub-account type must match parent account type");
                    }

                    account.Level = parentAccount.Level + 1;
                }
                else
                {
                    account.Level = 0;
                }
            }

            account.AccountCode = r.AccountCode;
            account.AccountName = r.AccountName;
            account.AccountType = r.AccountType;
            account.Description = r.Description;
            account.ParentAccountId = r.ParentAccountId;
            account.IsHeader = r.IsHeader;
            account.IsActive = r.IsActive;
            account.OpeningBalance = r.OpeningBalance;
            account.Currency = r.Currency;
            account.NormalBalance = ChartOfAccountHelper.GetNormalBalance(r.AccountType);
            account.UpdatedBy = request.UserId.ToString();
            account.UpdatedAt = DateTime.UtcNow;

            _coaRepository.Update(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (r.ParentAccountId != oldParentId)
            {
                await RecalculateChildLevelsAsync(account.AccountId, cancellationToken);
            }
        }

        private async Task<bool> IsCircularReferenceAsync(Guid accountId, Guid? newParentId, CancellationToken cancellationToken)
        {
            if (!newParentId.HasValue)
                return false;

            var descendants = await GetAllDescendantsAsync(accountId, cancellationToken);
            return descendants.Any(x => x.AccountId == newParentId.Value);
        }

        private async Task<List<ChartOfAccountEntity>> GetAllDescendantsAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var descendants = new List<ChartOfAccountEntity>();
            var children = await _coaRepository.FindAsync(
                x => x.ParentAccountId == accountId && !x.IsDeleted,
                cancellationToken);

            descendants.AddRange(children);

            foreach (var child in children)
            {
                var childDescendants = await GetAllDescendantsAsync(child.AccountId, cancellationToken);
                descendants.AddRange(childDescendants);
            }

            return descendants;
        }

        private async Task RecalculateChildLevelsAsync(Guid parentId, CancellationToken cancellationToken)
        {
            var children = await _coaRepository.FindAsync(
                x => x.ParentAccountId == parentId && !x.IsDeleted,
                cancellationToken);

            foreach (var child in children)
            {
                child.Level = await CalculateLevelAsync(child.AccountId, cancellationToken);
                _coaRepository.Update(child);
                await RecalculateChildLevelsAsync(child.AccountId, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<int> CalculateLevelAsync(Guid accountId, CancellationToken cancellationToken)
        {
            var account = await _coaRepository.FirstOrDefaultAsync(
                x => x.AccountId == accountId && !x.IsDeleted,
                cancellationToken);

            if (account == null || !account.ParentAccountId.HasValue)
                return 0;

            return 1 + await CalculateLevelAsync(account.ParentAccountId.Value, cancellationToken);
        }
    }
}