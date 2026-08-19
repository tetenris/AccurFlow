using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class FixRoleTypeDataCommandHandler : IRequestHandler<FixRoleTypeDataCommand>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FixRoleTypeDataCommandHandler(
            IRepository<RoleEntity> roleRepository,
            IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(FixRoleTypeDataCommand request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.FindAsync(
                x => !x.IsDeleted && x.RoleType == 0,
                cancellationToken);

            foreach (var role in roles)
            {
                var roleName = role.RoleName.ToLower();
                if (roleName.Contains("admin"))
                    role.RoleType = RoleEnum.Administrator;
                else if (roleName.Contains("accountant"))
                    role.RoleType = RoleEnum.Accountant;
                else if (roleName.Contains("manager"))
                    role.RoleType = RoleEnum.Manager;
                else if (roleName.Contains("finance"))
                    role.RoleType = RoleEnum.FinanceStaff;
                else if (roleName.Contains("ar"))
                    role.RoleType = RoleEnum.AROfficer;
                else if (roleName.Contains("ap"))
                    role.RoleType = RoleEnum.APOfficer;
                else if (roleName.Contains("purchasing"))
                    role.RoleType = RoleEnum.Purchasing;
                else if (roleName.Contains("sales"))
                    role.RoleType = RoleEnum.Sales;
                else if (roleName.Contains("warehouse"))
                    role.RoleType = RoleEnum.Warehouse;
                else if (roleName.Contains("viewer") || roleName.Contains("user"))
                    role.RoleType = RoleEnum.Viewer;
            }

            if (roles.Any())
            {
                _roleRepository.UpdateRange(roles);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}