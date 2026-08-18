using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitViewModel?>
    {
        private readonly IRepository<UnitEntity> _unitRepository;

        public GetUnitByIdQueryHandler(IRepository<UnitEntity> unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<UnitViewModel?> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitRepository.Query()
                .Where(x => x.UnitId == request.Id && !x.IsDeleted)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}