using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSupplierCommandHandler(
            IRepository<SupplierEntity> supplierRepository,
            IUnitOfWork unitOfWork)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.FirstOrDefaultAsync(
                x => x.SupplierId == request.SupplierId && !x.IsDeleted,
                cancellationToken);

            if (supplier == null)
                throw new Exception("Supplier not found");

            supplier.IsDeleted = true;
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.DeletedBy = request.UserId.ToString();

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}