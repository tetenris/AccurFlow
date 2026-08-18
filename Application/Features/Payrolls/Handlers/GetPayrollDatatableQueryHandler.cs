using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Payrolls.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payroll;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Payrolls.Handlers
{
    public class GetPayrollDatatableQueryHandler : IRequestHandler<GetPayrollDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<PayrollEntity> _payrollRepository;

        public GetPayrollDatatableQueryHandler(IRepository<PayrollEntity> payrollRepository)
        {
            _payrollRepository = payrollRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetPayrollDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _payrollRepository.Query()
                .Include(x => x.JournalEntry)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.PayrollNumber.ToLower().Contains(search)
                    || x.Notes != null && x.Notes.ToLower().Contains(search));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new PayrollViewModel
                {
                    PayrollId = x.PayrollId,
                    PayrollNumber = x.PayrollNumber,
                    PeriodMonth = x.PeriodMonth,
                    PeriodYear = x.PeriodYear,
                    PayrollDate = x.PayrollDate,
                    Status = x.Status,
                    TotalAllowances = x.TotalAllowances,
                    TotalDeductions = x.TotalDeductions,
                    TotalNetSalary = x.TotalNetSalary,
                    JournalNumber = x.JournalEntry != null ? x.JournalEntry.JournalNumber : null,
                    Notes = x.Notes,
                    LineCount = x.Lines.Count
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}