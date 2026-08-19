using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISuperAdminCheck _superAdminCheck;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserCommandHandler(
            IRepository<UserEntity> userRepository,
            IRepository<RoleEntity> roleRepository,
            IUnitOfWork unitOfWork,
            ISuperAdminCheck superAdminCheck,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _superAdminCheck = superAdminCheck;
            _currentUserService = currentUserService;
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;

            var user = await _userRepository.FirstOrDefaultAsync(u => u.UserId == model.UserId && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var currentRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleId == user.RoleId && !r.IsDeleted, cancellationToken);
            if (currentRole?.RoleType == RoleEnum.SuperAdministrator && !await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken))
            {
                throw new Exception("Super Administrator user cannot be edited from User Management");
            }

            var existingEmail = await _userRepository.FirstOrDefaultAsync(u => u.Email == model.Email && u.UserId != model.UserId && !u.IsDeleted, cancellationToken);
            if (existingEmail != null)
            {
                throw new Exception("Email already exists");
            }

            var existingUsername = await _userRepository.FirstOrDefaultAsync(u => u.UserName == model.UserName && u.UserId != model.UserId && !u.IsDeleted, cancellationToken);
            if (existingUsername != null)
            {
                throw new Exception("Username already exists");
            }

            var roleExists = await _roleRepository.AnyAsync(r => r.RoleId == model.RoleId && !r.IsDeleted, cancellationToken);
            if (!roleExists)
            {
                throw new Exception("Selected role does not exist");
            }

            await EnsureCanAssignRoleAsync(model.RoleId, cancellationToken);

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            user.UpdatedBy = _currentUserService.UserId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task EnsureCanAssignRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var targetRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted, cancellationToken);

            if (targetRole?.RoleType == RoleEnum.SuperAdministrator && !await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken))
            {
                throw new Exception("Only Super Administrator can assign Super Administrator role");
            }
        }
    }
}
