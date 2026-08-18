using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class ToggleCustomerStatusCommandHandler : IRequestHandler<ToggleCustomerStatusCommand>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleCustomerStatusCommandHandler(
            IRepository<CustomerEntity> customerRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ToggleCustomerStatusCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(
                x => x.CustomerId == request.CustomerId && !x.IsDeleted,
                cancellationToken);

            if (customer == null)
                throw new Exception("Customer not found");

            customer.IsActive = !customer.IsActive;
            customer.UpdatedBy = request.UserId.ToString();
            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}