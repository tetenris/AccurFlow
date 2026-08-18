using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Employees.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Payroll;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Employees.Handlers
{
    public class GetEmployeeDatatableQueryHandler : IRequestHandler<GetEmployeeDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<EmployeeEntity> _employeeRepository;

        public GetEmployeeDatatableQueryHandler(IRepository<EmployeeEntity> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetEmployeeDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var query = _employeeRepository.Query().Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x => x.EmployeeCode.ToLower().Contains(search)
                    || x.FullName.ToLower().Contains(search)
                    || (x.Position != null && x.Position.ToLower().Contains(search))
                    || (x.Department != null && x.Department.ToLower().Contains(search)));
            }

            var total = await query.CountAsync(cancellationToken);
            var data = await query
                .OrderBy(x => x.EmployeeCode)
                .Skip((r.Page - 1) * r.Size).Take(r.Size)
                .Select(x => new EmployeeViewModel
                {
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.EmployeeCode,
                    FullName = x.FullName,
                    Position = x.Position,
                    Department = x.Department,
                    HireDate = x.HireDate,
                    BasicSalary = x.BasicSalary,
                    BankAccountNumber = x.BankAccountNumber,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);

            return new BaseDatatableResponse { Draw = r.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }
    }
}