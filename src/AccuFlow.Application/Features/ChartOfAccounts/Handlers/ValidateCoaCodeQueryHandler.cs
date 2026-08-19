using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class ValidateCoaCodeQueryHandler : IRequestHandler<ValidateCoaCodeQuery, bool>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public ValidateCoaCodeQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<bool> Handle(ValidateCoaCodeQuery request, CancellationToken cancellationToken)
        {
            var isUnique = !await _coaRepository.AnyAsync(
                x => x.AccountCode == request.Code
                    && !x.IsDeleted
                    && (!request.ExcludeId.HasValue || x.AccountId != request.ExcludeId.Value),
                cancellationToken);

            return isUnique;
        }
    }
}