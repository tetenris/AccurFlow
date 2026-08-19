using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BillOfMaterials.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Production;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BillOfMaterials.Handlers
{
    public class GetBomByIdQueryHandler : IRequestHandler<GetBomByIdQuery, BomDetailViewModel?>
    {
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;

        public GetBomByIdQueryHandler(IRepository<BillOfMaterialEntity> bomRepository)
        {
            _bomRepository = bomRepository;
        }

        public async Task<BomDetailViewModel?> Handle(GetBomByIdQuery request, CancellationToken cancellationToken)
        {
            return await _bomRepository.Query()
                .Include(x => x.FinishedItem)
                .Include(x => x.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.ComponentItem)
                .Where(x => x.BomId == request.BomId && !x.IsDeleted)
                .Select(x => new BomDetailViewModel
                {
                    BomId = x.BomId,
                    BomNumber = x.BomNumber,
                    FinishedItemId = x.FinishedItemId,
                    FinishedItemCode = x.FinishedItem.ItemCode,
                    FinishedItemName = x.FinishedItem.ItemName,
                    IsActive = x.IsActive,
                    Notes = x.Notes,
                    Lines = x.Lines.OrderBy(l => l.ComponentItem.ItemCode).Select(l => new BomLineViewModel
                    {
                        BomLineId = l.BomLineId,
                        ComponentItemId = l.ComponentItemId,
                        ComponentItemCode = l.ComponentItem.ItemCode,
                        ComponentItemName = l.ComponentItem.ItemName,
                        QuantityPerUnit = l.QuantityPerUnit
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}