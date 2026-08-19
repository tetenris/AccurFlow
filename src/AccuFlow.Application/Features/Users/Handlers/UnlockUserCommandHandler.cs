using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISuperAdminCheck _superAdminCheck;
        private readonly ICurrentUserService _currentUserService;

        private const string DefaultPassword = "Qwerty@123";

        public UnlockUserCommandHandler(
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

        public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.Id && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var currentRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleId == user.RoleId && !r.IsDeleted, cancellationToken);
            if (currentRole?.RoleType == RoleEnum.SuperAdministrator && !await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken))
            {
                throw new Exception("Super Administrator user cannot be unlocked from User Management");
            }

            if (!user.IsLocked)
            {
                throw new Exception("User is not locked");
            }

            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
            user.LockedAt = null;
            user.LockedReason = null;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
            user.PasswordChangedAt = DateTime.UtcNow.AddDays(-31);
            user.PasswordExpiresAt = DateTime.UtcNow.AddDays(-1);
            user.UpdatedBy = _currentUserService.UserId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
