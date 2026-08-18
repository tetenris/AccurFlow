using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BillOfMaterials.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BillOfMaterials.Handlers
{
    public class CreateBomCommandHandler : IRequestHandler<CreateBomCommand>
    {
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBomCommandHandler(
            IRepository<BillOfMaterialEntity> bomRepository,
            IUnitOfWork unitOfWork)
        {
            _bomRepository = bomRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateBomCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;
            if (r.FinishedItemId == Guid.Empty) throw new Exception("Finished item is required");
            if (r.Lines == null || r.Lines.Count == 0) throw new Exception("At least one component line is required");
            if (r.Lines.Any(l => l.QuantityPerUnit <= 0)) throw new Exception("Component quantity must be greater than zero");
            if (r.Lines.Any(l => l.ComponentItemId == r.FinishedItemId)) throw new Exception("Component cannot be the same as finished item");

            var bom = new BillOfMaterialEntity
            {
                BomId = Guid.NewGuid(),
                BomNumber = await GenerateBomNumberAsync(cancellationToken),
                FinishedItemId = r.FinishedItemId,
                Notes = r.Notes,
                IsActive = true,
                CreatedBy = userId.ToString()
            };

            foreach (var line in r.Lines)
            {
                bom.Lines.Add(new BillOfMaterialLineEntity
                {
                    BomLineId = Guid.NewGuid(),
                    ComponentItemId = line.ComponentItemId,
                    QuantityPerUnit = line.QuantityPerUnit,
                    CreatedBy = userId.ToString()
                });
            }

            _bomRepository.Add(bom);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateBomNumberAsync(CancellationToken cancellationToken)
        {
            var last = await _bomRepository.Query()
                .Where(x => x.BomNumber.StartsWith("BOM-"))
                .OrderByDescending(x => x.BomNumber)
                .Select(x => x.BomNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = string.IsNullOrEmpty(last) ? 1 : int.Parse(last[4..]) + 1;
            return $"BOM-{next:D5}";
        }
    }
}