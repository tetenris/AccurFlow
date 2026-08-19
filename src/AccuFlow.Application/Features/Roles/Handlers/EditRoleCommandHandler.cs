using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class EditRoleCommandHandler : IRequestHandler<EditRoleCommand>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EditRoleCommandHandler(
            IRepository<RoleEntity> roleRepository,
            IRepository<UserEntity> userRepository,
            IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(EditRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(
                x => x.RoleId == request.Request.RoleId && !x.IsDeleted,
                cancellationToken);

            if (role == null)
                throw new Exception("Role not found");

            if (role.RoleType == RoleEnum.SuperAdministrator)
                throw new Exception("Super Administrator role cannot be edited from Role Management");

            var activeUsersWithRole = await _userRepository.FindAsync(
                u => u.RoleId == request.Request.RoleId && !u.IsDeleted && u.IsActive,
                cancellationToken);

            if (role.RoleType != (RoleEnum)request.Request.RoleType)
            {
                if (activeUsersWithRole.Any())
                {
                    throw new Exception($"Cannot change role type. There are {activeUsersWithRole.Count} active user(s) assigned to this role. Please reassign these users first.");
                }

                var existingRole = await _roleRepository.FirstOrDefaultAsync(
                    x => x.RoleType == (RoleEnum)request.Request.RoleType
                        && x.RoleId != request.Request.RoleId
                        && !x.IsDeleted,
                    cancellationToken);

                if (existingRole != null)
                {
                    throw new Exception($"Role type '{((RoleEnum)request.Request.RoleType).ToString()}' already exists");
                }

                role.RoleType = (RoleEnum)request.Request.RoleType;
                role.RoleName = request.Request.RoleName;
            }

            if (role.IsActive && !request.Request.IsActive)
            {
                if (activeUsersWithRole.Any())
                {
                    var userNames = string.Join(", ", activeUsersWithRole.Select(u => u.UserName).Take(5));
                    var moreUsers = activeUsersWithRole.Count > 5 ? $" and {activeUsersWithRole.Count - 5} more" : "";
                    throw new Exception($"Cannot deactivate role. There are {activeUsersWithRole.Count} active user(s) assigned to this role ({userNames}{moreUsers}). Please reassign or deactivate these users first.");
                }
            }

            role.Description = request.Request.Description ?? string.Empty;
            role.IsActive = request.Request.IsActive;

            var userIdStr = request.UserId.ToString();
            role.UpdatedBy = userIdStr;
            role.UpdatedAt = DateTime.UtcNow;

            _roleRepository.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}