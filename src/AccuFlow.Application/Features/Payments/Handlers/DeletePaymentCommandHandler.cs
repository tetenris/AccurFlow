using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payments.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand>
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePaymentCommandHandler(
            IRepository<PaymentEntity> paymentRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.Query()
                .Include(x => x.Allocations)
                .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId && !x.IsDeleted, cancellationToken);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be deleted");
            payment.IsDeleted = true;
            payment.DeletedBy = request.UserId.ToString();
            payment.DeletedAt = DateTime.UtcNow;
            foreach (var allocation in payment.Allocations)
            {
                allocation.IsDeleted = true;
                allocation.DeletedBy = request.UserId.ToString();
                allocation.DeletedAt = DateTime.UtcNow;
            }
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}