using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCustomerCommandHandler(
            IRepository<CustomerEntity> customerRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                x => x.CustomerCode == r.CustomerCode && !x.IsDeleted,
                cancellationToken);

            if (existingCustomer != null)
            {
                throw new Exception($"Customer code '{r.CustomerCode}' already exists");
            }

            var customer = new CustomerEntity
            {
                CustomerId = Guid.NewGuid(),
                CustomerCode = r.CustomerCode,
                CustomerName = r.CustomerName,
                CustomerType = r.CustomerType,
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

            _customerRepository.Add(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}