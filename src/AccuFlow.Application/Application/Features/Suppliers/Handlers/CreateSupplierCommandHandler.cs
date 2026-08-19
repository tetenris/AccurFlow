using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateSupplierCommandHandler(
            IRepository<SupplierEntity> supplierRepository,
            IUnitOfWork unitOfWork)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var existingSupplier = await _supplierRepository.FirstOrDefaultAsync(
                x => x.SupplierCode == r.SupplierCode && !x.IsDeleted,
                cancellationToken);

            if (existingSupplier != null)
            {
                throw new Exception($"Supplier code '{r.SupplierCode}' already exists");
            }

            var supplier = new SupplierEntity
            {
                SupplierId = Guid.NewGuid(),
                SupplierCode = r.SupplierCode,
                SupplierName = r.SupplierName,
                SupplierType = r.SupplierType,
                ContactPerson = r.ContactPerson,
                Phone = r.Phone,
                Email = r.Email,
                Website = r.Website,
                Address = r.Address,
                City = r.City,
                State = r.State,
                PostalCode = r.PostalCode,
                Country = r.Country,
                CreditLimit = r.CreditLimit,
                PaymentTerms = r.PaymentTerms,
                CurrentBalance = 0,
                TaxId = r.TaxId,
                Notes = r.Notes,
                IsActive = r.IsActive,
                CreatedBy = request.UserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _supplierRepository.Add(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}