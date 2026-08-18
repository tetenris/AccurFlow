using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISuperAdminCheck _superAdminCheck;
        private readonly AccuFlow.Infrastructures.ICurrentUserService _currentUserService;

        private const string DefaultPassword = "Qwerty@123";

        public CreateUserCommandHandler(
            IRepository<UserEntity> userRepository,
            IRepository<RoleEntity> roleRepository,
            IUnitOfWork unitOfWork,
            ISuperAdminCheck superAdminCheck,
            AccuFlow.Infrastructures.ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _superAdminCheck = superAdminCheck;
            _currentUserService = currentUserService;
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;

            var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted, cancellationToken);
            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }

            var existingUsername = await _userRepository.FirstOrDefaultAsync(u => u.UserName == model.UserName && !u.IsDeleted, cancellationToken);
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

            var user = new UserEntity
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                PasswordChangedAt = DateTime.UtcNow.AddDays(-31),
                PasswordExpiresAt = DateTime.UtcNow.AddDays(-1),
                FullName = model.FullName,
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                CreatedBy = _currentUserService.UserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.Add(user);
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