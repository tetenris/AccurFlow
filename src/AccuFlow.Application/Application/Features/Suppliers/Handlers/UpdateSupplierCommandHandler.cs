using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSupplierCommandHandler(
            IRepository<SupplierEntity> supplierRepository,
            IUnitOfWork unitOfWork)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var supplier = await _supplierRepository.FirstOrDefaultAsync(
                x => x.SupplierId == r.SupplierId && !x.IsDeleted,
                cancellationToken);

            if (supplier == null)
                throw new Exception("Supplier not found");

            if (supplier.SupplierCode != r.SupplierCode)
            {
                var existingSupplier = await _supplierRepository.FirstOrDefaultAsync(
                    x => x.SupplierCode == r.SupplierCode
                        && x.SupplierId != r.SupplierId
                        && !x.IsDeleted,
                    cancellationToken);

                if (existingSupplier != null)
                {
                    throw new Exception($"Supplier code '{r.SupplierCode}' already exists");
                }
            }

            supplier.SupplierCode = r.SupplierCode;
            supplier.SupplierName = r.SupplierName;
            supplier.SupplierType = r.SupplierType;
            supplier.ContactPerson = r.ContactPerson;
            supplier.Phone = r.Phone;
            supplier.Email = r.Email;
            supplier.Website = r.Website;
            supplier.Address = r.Address;
            supplier.City = r.City;
            supplier.State = r.State;
            supplier.PostalCode = r.PostalCode;
            supplier.Country = r.Country;
            supplier.CreditLimit = r.CreditLimit;
            supplier.PaymentTerms = r.PaymentTerms;
            supplier.TaxId = r.TaxId;
            supplier.Notes = r.Notes;
            supplier.IsActive = r.IsActive;
            supplier.UpdatedBy = request.UserId.ToString();
            supplier.UpdatedAt = DateTime.UtcNow;

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}