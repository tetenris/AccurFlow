using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.Customer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerViewModel?>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<UserEntity> _userRepository;

        public GetCustomerByIdQueryHandler(
            IRepository<CustomerEntity> customerRepository,
            IRepository<UserEntity> userRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        public async Task<CustomerViewModel?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.Query()
                .Where(x => x.CustomerId == request.CustomerId && !x.IsDeleted)
                .Select(x => new CustomerViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerType = x.CustomerType,
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

            if (customer != null)
            {
                customer.CreatedBy = await ResolveUserNameAsync(customer.CreatedBy ?? string.Empty, cancellationToken);
                if (!string.IsNullOrEmpty(customer.UpdatedBy))
                {
                    customer.UpdatedBy = await ResolveUserNameAsync(customer.UpdatedBy, cancellationToken);
                }
            }

            return customer;
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