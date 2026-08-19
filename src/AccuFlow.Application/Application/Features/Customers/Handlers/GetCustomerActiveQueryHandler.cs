using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Customer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class GetCustomerActiveQueryHandler : IRequestHandler<GetCustomerActiveQuery, List<CustomerDropdownViewModel>>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;

        public GetCustomerActiveQueryHandler(IRepository<CustomerEntity> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerDropdownViewModel>> Handle(GetCustomerActiveQuery request, CancellationToken cancellationToken)
        {
            return await _customerRepository.Query()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.CustomerCode)
                .Select(x => new CustomerDropdownViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName
                })
                .ToListAsync(cancellationToken);
        }
    }
}