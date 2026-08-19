using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Supplier;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IRepository<UserEntity> _userRepository;

        public GetSupplierByIdQueryHandler(
            IRepository<SupplierEntity> supplierRepository,
            IRepository<UserEntity> userRepository)
        {
            _supplierRepository = supplierRepository;
            _userRepository = userRepository;
        }

        public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
        {
            var supplier = await _supplierRepository.Query()
                .Where(x => x.SupplierId == request.SupplierId && !x.IsDeleted)
                .Select(x => new SupplierViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    SupplierType = x.SupplierType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Address = x.Address,
                    City = x.City,
                    State = x.State,
                    PostalCode = x.PostalCode,
                    Country = x.Country,
                    CreditLimit = x.CreditLimit,
                    PaymentTerms = x.PaymentTerms,
                    CurrentBalance = x.CurrentBalance,
                    TaxId = x.TaxId,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (supplier != null)
            {
                supplier.CreatedBy = await ResolveUserNameAsync(supplier.CreatedBy ?? string.Empty, cancellationToken);
                if (!string.IsNullOrEmpty(supplier.UpdatedBy))
                {
                    supplier.UpdatedBy = await ResolveUserNameAsync(supplier.UpdatedBy, cancellationToken);
                }
            }

            return supplier;
        }

        private async Task<string> ResolveUserNameAsync(string userIdString, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return "System";
            }

            var name = await _userRepository.Query()
                .Where(u => u.UserId == userId)
                .Select(u => u.FullName ?? u.UserName)
                .FirstOrDefaultAsync(cancellationToken);

            return name ?? "System";
        }
    }
}