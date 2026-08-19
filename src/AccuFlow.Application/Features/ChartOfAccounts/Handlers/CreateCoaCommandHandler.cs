using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class CreateCoaCommandHandler : IRequestHandler<CreateCoaCommand>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCoaCommandHandler(
            IRepository<ChartOfAccountEntity> coaRepository,
            IUnitOfWork unitOfWork)
        {
            _coaRepository = coaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateCoaCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            if (await _coaRepository.AnyAsync(x => x.AccountCode == r.AccountCode && !x.IsDeleted, cancellationToken))
            {
                throw new Exception($"Account code '{r.AccountCode}' already exists");
            }

            ChartOfAccountEntity? parentAccount = null;
            int level = 0;

            if (r.ParentAccountId.HasValue)
            {
                parentAccount = await _coaRepository.FirstOrDefaultAsync(
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

                level = parentAccount.Level + 1;
            }

            var account = new ChartOfAccountEntity
            {
                AccountId = Guid.NewGuid(),
                AccountCode = r.AccountCode,
                AccountName = r.AccountName,
                AccountType = r.AccountType,
                Description = r.Description,
                ParentAccountId = r.ParentAccountId,
                IsHeader = r.IsHeader,
                IsActive = r.IsActive,
                OpeningBalance = r.OpeningBalance,
                NormalBalance = ChartOfAccountHelper.GetNormalBalance(r.AccountType),
                Currency = r.Currency,
                Level = level,
                CreatedBy = request.UserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _coaRepository.Add(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}