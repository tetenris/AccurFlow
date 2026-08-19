using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Supplier;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class GetSupplierActiveQueryHandler : IRequestHandler<GetSupplierActiveQuery, List<SupplierDropdownViewModel>>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;

        public GetSupplierActiveQueryHandler(IRepository<SupplierEntity> supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<List<SupplierDropdownViewModel>> Handle(GetSupplierActiveQuery request, CancellationToken cancellationToken)
        {
            return await _supplierRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.SupplierCode)
                .Select(x => new SupplierDropdownViewModel
                {
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName
                })
                .ToListAsync(cancellationToken);
        }
    }
}