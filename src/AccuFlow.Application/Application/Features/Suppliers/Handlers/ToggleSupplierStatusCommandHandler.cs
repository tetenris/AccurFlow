using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class ToggleSupplierStatusCommandHandler : IRequestHandler<ToggleSupplierStatusCommand>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleSupplierStatusCommandHandler(
            IRepository<SupplierEntity> supplierRepository,
            IUnitOfWork unitOfWork)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ToggleSupplierStatusCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.FirstOrDefaultAsync(
                x => x.SupplierId == request.SupplierId && !x.IsDeleted,
                cancellationToken);

            if (supplier == null)
                throw new Exception("Supplier not found");

            supplier.IsActive = !supplier.IsActive;
            supplier.UpdatedBy = request.UserId.ToString();
            supplier.UpdatedAt = DateTime.UtcNow;

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}