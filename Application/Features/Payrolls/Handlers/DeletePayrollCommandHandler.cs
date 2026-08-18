using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payrolls.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payrolls.Handlers
{
    public class DeletePayrollCommandHandler : IRequestHandler<DeletePayrollCommand>
    {
        private readonly IRepository<PayrollEntity> _payrollRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePayrollCommandHandler(
            IRepository<PayrollEntity> payrollRepository,
            IUnitOfWork unitOfWork)
        {
            _payrollRepository = payrollRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeletePayrollCommand request, CancellationToken cancellationToken)
        {
            var payroll = await _payrollRepository.Query()
                .FirstOrDefaultAsync(x => x.PayrollId == request.PayrollId && !x.IsDeleted, cancellationToken);
            if (payroll == null) throw new Exception("Payroll not found");
            if (payroll.Status != "Draft") throw new Exception("Only draft payroll can be deleted");

            payroll.IsDeleted = true;
            payroll.DeletedAt = DateTime.UtcNow;
            payroll.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}