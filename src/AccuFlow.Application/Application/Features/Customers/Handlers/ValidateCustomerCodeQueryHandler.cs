using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class ValidateCustomerCodeQueryHandler : IRequestHandler<ValidateCustomerCodeQuery, bool>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;

        public ValidateCustomerCodeQueryHandler(IRepository<CustomerEntity> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<bool> Handle(ValidateCustomerCodeQuery request, CancellationToken cancellationToken)
        {
            var isUnique = !await _customerRepository.AnyAsync(
                x => x.CustomerCode == request.Code
                    && !x.IsDeleted
                    && (!request.ExcludeId.HasValue || x.CustomerId != request.ExcludeId.Value),
                cancellationToken);

            return isUnique;
        }
    }
}