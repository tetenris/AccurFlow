using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BillOfMaterials.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Production;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BillOfMaterials.Handlers
{
    public class GetBomsQueryHandler : IRequestHandler<GetBomsQuery, List<BomViewModel>>
    {
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;

        public GetBomsQueryHandler(IRepository<BillOfMaterialEntity> bomRepository)
        {
            _bomRepository = bomRepository;
        }

        public async Task<List<BomViewModel>> Handle(GetBomsQuery request, CancellationToken cancellationToken)
        {
            return await _bomRepository.Query()
                .Include(x => x.FinishedItem)
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new BomViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    LineCount = x.Lines.Count,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);
        }
    }
}