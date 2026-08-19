using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class GetActiveUnitsQueryHandler : IRequestHandler<GetActiveUnitsQuery, List<UnitViewModel>>
    {
        private readonly IRepository<UnitEntity> _unitRepository;

        public GetActiveUnitsQueryHandler(IRepository<UnitEntity> unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<List<UnitViewModel>> Handle(GetActiveUnitsQuery request, CancellationToken cancellationToken)
        {
            return await _unitRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.UnitCode)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);
        }
    }
}