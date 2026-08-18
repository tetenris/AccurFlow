using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.BillOfMaterials.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.BillOfMaterials.Handlers
{
    public class DeleteBomCommandHandler : IRequestHandler<DeleteBomCommand>
    {
        private readonly IRepository<BillOfMaterialEntity> _bomRepository;
        private readonly IRepository<ProductionOrderEntity> _productionOrderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBomCommandHandler(
            IRepository<BillOfMaterialEntity> bomRepository,
            IRepository<ProductionOrderEntity> productionOrderRepository,
            IUnitOfWork unitOfWork)
        {
            _bomRepository = bomRepository;
            _productionOrderRepository = productionOrderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteBomCommand request, CancellationToken cancellationToken)
        {
            var bom = await _bomRepository.Query()
                .FirstOrDefaultAsync(x => x.BomId == request.BomId && !x.IsDeleted, cancellationToken);
            if (bom == null) throw new Exception("Bill of material not found");

            var usedInOrder = await _productionOrderRepository.Query()
                .AnyAsync(x => x.BomId == request.BomId && !x.IsDeleted && x.Status != "Draft", cancellationToken);
            if (usedInOrder) throw new Exception("Cannot delete BOM that already has posted production orders");

            bom.IsDeleted = true;
            bom.DeletedAt = DateTime.UtcNow;
            bom.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}