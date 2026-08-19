using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCustomerCommandHandler(
            IRepository<CustomerEntity> customerRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var customer = await _customerRepository.FirstOrDefaultAsync(
                x => x.CustomerId == r.CustomerId && !x.IsDeleted,
                cancellationToken);

            if (customer == null)
                throw new Exception("Customer not found");

            if (customer.CustomerCode != r.CustomerCode)
            {
                var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                    x => x.CustomerCode == r.CustomerCode
                        && x.CustomerId != r.CustomerId
                        && !x.IsDeleted,
                    cancellationToken);

                if (existingCustomer != null)
                {
                    throw new Exception($"Customer code '{r.CustomerCode}' already exists");
                }
            }

            customer.CustomerCode = r.CustomerCode;
            customer.CustomerName = r.CustomerName;
            customer.CustomerType = r.CustomerType;
            customer.ContactPerson = r.ContactPerson;
            customer.Phone = r.Phone;
            customer.Email = r.Email;
            customer.Website = r.Website;
            customer.Address = r.Address;
            customer.City = r.City;
            customer.State = r.State;
            customer.PostalCode = r.PostalCode;
            customer.Country = r.Country;
            customer.CreditLimit = r.CreditLimit;
            customer.PaymentTerms = r.PaymentTerms;
            customer.TaxId = r.TaxId;
            customer.Notes = r.Notes;
            customer.IsActive = r.IsActive;
            customer.UpdatedBy = request.UserId.ToString();
            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}