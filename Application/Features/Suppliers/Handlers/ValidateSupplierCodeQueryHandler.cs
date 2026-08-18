using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class ValidateSupplierCodeQueryHandler : IRequestHandler<ValidateSupplierCodeQuery, bool>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;

        public ValidateSupplierCodeQueryHandler(IRepository<SupplierEntity> supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<bool> Handle(ValidateSupplierCodeQuery request, CancellationToken cancellationToken)
        {
            var isUnique = !await _supplierRepository.AnyAsync(
                x => x.SupplierCode == request.Code
                    && !x.IsDeleted
                    && (!request.ExcludeId.HasValue || x.SupplierId != request.ExcludeId.Value),
                cancellationToken);

            return isUnique;
        }
    }
}