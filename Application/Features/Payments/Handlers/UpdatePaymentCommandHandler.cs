using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payments.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand>
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePaymentCommandHandler(
            IRepository<PaymentEntity> paymentRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var payment = await _paymentRepository.Query()
                .Include(x => x.Allocations)
                .FirstOrDefaultAsync(x => x.PaymentId == r.PaymentId && !x.IsDeleted, cancellationToken);
            if (payment == null) throw new Exception("Payment not found");
            if (payment.Status != "Draft") throw new Exception("Only draft payment can be edited");
            if (!r.Allocations.Any()) throw new Exception("Payment allocation is required");

            payment.PaymentType = r.PaymentType;
            payment.PaymentDate = r.PaymentDate;
            payment.CustomerId = r.PaymentType == "Receipt" ? r.CustomerId : null;
            payment.SupplierId = r.PaymentType == "Payment" ? r.SupplierId : null;
            payment.CashBankAccountId = r.CashBankAccountId;
            payment.PaymentMethod = r.PaymentMethod;
            payment.ReferenceNumber = r.ReferenceNumber;
            payment.Notes = r.Notes;
            payment.TotalAmount = 0;
            payment.UpdatedBy = userId.ToString();
            payment.UpdatedAt = DateTime.UtcNow;

            foreach (var oldAllocation in payment.Allocations)
            {
                oldAllocation.IsDeleted = true;
                oldAllocation.DeletedBy = userId.ToString();
                oldAllocation.DeletedAt = DateTime.UtcNow;
            }

            foreach (var allocation in r.Allocations)
            {
                payment.Allocations.Add(new PaymentAllocationEntity
                {
                    PaymentAllocationId = Guid.NewGuid(),
                    InvoiceId = allocation.InvoiceId,
                    AllocatedAmount = allocation.AllocatedAmount,
                    CreatedBy = userId.ToString()
                });
                payment.TotalAmount += allocation.AllocatedAmount;
            }

            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}