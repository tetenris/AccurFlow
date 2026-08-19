using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class GetUnitDatatableQueryHandler : IRequestHandler<GetUnitDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<UnitEntity> _unitRepository;

        public GetUnitDatatableQueryHandler(IRepository<UnitEntity> unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetUnitDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _unitRepository.Query().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.UnitCode.ToLower().Contains(search) || x.UnitName.ToLower().Contains(search));
            }
            var total = await query.CountAsync(cancellationToken);
            var data = await query.OrderBy(x => x.UnitCode).Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new UnitViewModel
                {
                    UnitId = x.UnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    Description = x.Description,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);
            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}