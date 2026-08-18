using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Suppliers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Suppliers.Handlers
{
    public class GenerateSupplierCodeQueryHandler : IRequestHandler<GenerateSupplierCodeQuery, string>
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;

        public GenerateSupplierCodeQueryHandler(IRepository<SupplierEntity> supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<string> Handle(GenerateSupplierCodeQuery request, CancellationToken cancellationToken)
        {
            var lastSupplier = await _supplierRepository.Query()
                .Where(x => x.SupplierCode.StartsWith("SUPP-"))
                .OrderByDescending(x => x.SupplierCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastSupplier == null)
            {
                return "SUPP-00001";
            }

            var lastCode = lastSupplier.SupplierCode;
            var lastNumber = int.Parse(lastCode.Substring(5));
            var newNumber = lastNumber + 1;

            return $"SUPP-{newNumber:D5}";
        }
    }
}