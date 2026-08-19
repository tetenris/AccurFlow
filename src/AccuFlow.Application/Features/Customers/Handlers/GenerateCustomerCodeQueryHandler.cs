using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class GenerateCustomerCodeQueryHandler : IRequestHandler<GenerateCustomerCodeQuery, string>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;

        public GenerateCustomerCodeQueryHandler(IRepository<CustomerEntity> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<string> Handle(GenerateCustomerCodeQuery request, CancellationToken cancellationToken)
        {
            var lastCustomer = await _customerRepository.Query()
                .Where(x => x.CustomerCode.StartsWith("CUST-"))
                .OrderByDescending(x => x.CustomerCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastCustomer == null)
            {
                return "CUST-00001";
            }

            var lastCode = lastCustomer.CustomerCode;
            var lastNumber = int.Parse(lastCode.Substring(5));
            var newNumber = lastNumber + 1;

            return $"CUST-{newNumber:D5}";
        }
    }
}