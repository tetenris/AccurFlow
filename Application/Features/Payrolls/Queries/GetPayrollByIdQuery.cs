using AccuFlow.Models.Payroll;
using MediatR;

namespace AccuFlow.Application.Features.Payrolls.Queries
{
    public record GetPayrollByIdQuery(Guid PayrollId) : IRequest<PayrollDetailViewModel?>;
}