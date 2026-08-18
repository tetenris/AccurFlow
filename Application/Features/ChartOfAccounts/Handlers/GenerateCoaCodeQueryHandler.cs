using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ChartOfAccounts.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ChartOfAccounts.Handlers
{
    public class GenerateCoaCodeQueryHandler : IRequestHandler<GenerateCoaCodeQuery, string>
    {
        private readonly IRepository<ChartOfAccountEntity> _coaRepository;

        public GenerateCoaCodeQueryHandler(IRepository<ChartOfAccountEntity> coaRepository)
        {
            _coaRepository = coaRepository;
        }

        public async Task<string> Handle(GenerateCoaCodeQuery request, CancellationToken cancellationToken)
        {
            string prefix = ChartOfAccountHelper.GetAccountTypePrefix(request.AccountType);

            if (request.ParentId.HasValue)
            {
                var parent = await _coaRepository.FirstOrDefaultAsync(
                    x => x.AccountId == request.ParentId.Value && !x.IsDeleted,
                    cancellationToken);

                if (parent != null)
                {
                    var lastChild = await _coaRepository.Query()
                        .Where(x => x.ParentAccountId == request.ParentId.Value && !x.IsDeleted)
                        .OrderByDescending(x => x.AccountCode)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (lastChild != null)
                    {
                        var parts = lastChild.AccountCode.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                        {
                            return $"{prefix}-{(number + 100):D5}";
                        }
                    }

                    var parentParts = parent.AccountCode.Split('-');
                    if (parentParts.Length == 2 && int.TryParse(parentParts[1], out int parentNumber))
                    {
                        return $"{prefix}-{(parentNumber + 100):D5}";
                    }
                }
            }

            var lastRoot = await _coaRepository.Query()
                .Where(x => x.ParentAccountId == null && x.AccountType == request.AccountType && !x.IsDeleted)
                .OrderByDescending(x => x.AccountCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastRoot != null)
            {
                var parts = lastRoot.AccountCode.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    return $"{prefix}-{(number + 10000):D5}";
                }
            }

            return $"{prefix}-10000";
        }
    }
}