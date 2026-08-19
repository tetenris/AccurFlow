using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payments.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payments.Handlers
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand>
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePaymentCommandHandler(
            IRepository<PaymentEntity> paymentRepository,
            IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var userId = request.UserId;

            var payment = new PaymentEntity
            {
                PaymentId = Guid.NewGuid(),
                PaymentNumber = await GenerateNumber(r.PaymentType, cancellationToken),
                PaymentType = r.PaymentType,
                PaymentDate = r.PaymentDate,
                CustomerId = r.CustomerId,
                SupplierId = r.SupplierId,
                CashBankAccountId = r.CashBankAccountId,
                PaymentMethod = r.PaymentMethod,
                ReferenceNumber = r.ReferenceNumber,
                Notes = r.Notes,
                Status = "Draft",
                CreatedBy = userId.ToString()
            };

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

            _paymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateNumber(string paymentType, CancellationToken cancellationToken)
        {
            var prefix = paymentType == "Payment" ? "PAY" : "RCT";
            var last = await _paymentRepository.Query()
                .Where(x => x.PaymentNumber.StartsWith(prefix + "-"))
                .OrderByDescending(x => x.PaymentNumber)
                .FirstOrDefaultAsync(cancellationToken);
            var next = last == null ? 1 : int.Parse(last.PaymentNumber[(prefix.Length + 1)..]) + 1;
            return $"{prefix}-{next:D5}";
        }
    }
}