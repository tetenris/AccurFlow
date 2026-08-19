using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.JournalEntries.Commands;
using AccuFlow.Application.Features.Payrolls.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.JournalEntry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payrolls.Handlers
{
    public class PostPayrollCommandHandler : IRequestHandler<PostPayrollCommand>
    {
        private static readonly Guid SalaryExpenseAccountId = Guid.Parse("50000000-0000-0000-0000-000000000003");
        private static readonly Guid PayrollPayableAccountId = Guid.Parse("20000000-0000-0000-0000-000000000020");

        private readonly IRepository<PayrollEntity> _payrollRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISender _mediator;

        public PostPayrollCommandHandler(
            IRepository<PayrollEntity> payrollRepository,
            IUnitOfWork unitOfWork,
            ISender mediator)
        {
            _payrollRepository = payrollRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task Handle(PostPayrollCommand request, CancellationToken cancellationToken)
        {
            var payroll = await _payrollRepository.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.PayrollId == request.PayrollId && !x.IsDeleted, cancellationToken);
            if (payroll == null) throw new Exception("Payroll not found");
            if (payroll.Status != "Draft") throw new Exception("Only draft payroll can be posted");
            if (payroll.Lines.Count == 0) throw new Exception("Payroll has no salary lines");

            var description = $"Auto journal for payroll {payroll.PayrollNumber}";
            var journalId = await _mediator.Send(new CreateJournalCommand(new CreateJournalEntryRequest
            {
                JournalDate = payroll.PayrollDate,
                Description = description,
                JournalLines = new List<JournalLineRequest>
                {
                    new() { AccountId = SalaryExpenseAccountId, Description = description, DebitAmount = payroll.TotalNetSalary, CreditAmount = 0 },
                    new() { AccountId = PayrollPayableAccountId, Description = description, DebitAmount = 0, CreditAmount = payroll.TotalNetSalary }
                }
            }, request.UserId), cancellationToken);

            await _mediator.Send(new PostJournalCommand(new PostJournalRequest
            {
                JournalId = journalId,
                PostedDate = payroll.PayrollDate
            }, request.UserId), cancellationToken);

            payroll.JournalId = journalId;
            payroll.Status = "Posted";
            payroll.UpdatedAt = DateTime.UtcNow;
            payroll.UpdatedBy = request.UserId.ToString();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}