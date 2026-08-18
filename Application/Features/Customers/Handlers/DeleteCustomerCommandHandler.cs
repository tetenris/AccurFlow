using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCustomerCommandHandler(
            IRepository<CustomerEntity> customerRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.FirstOrDefaultAsync(
                x => x.CustomerId == request.CustomerId && !x.IsDeleted,
                cancellationToken);

            if (customer == null)
                throw new Exception("Customer not found");

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = request.UserId.ToString();

            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}