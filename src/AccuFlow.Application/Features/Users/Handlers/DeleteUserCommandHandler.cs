using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISuperAdminCheck _superAdminCheck;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(
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

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.Id && !u.IsDeleted, cancellationToken);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var currentRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleId == user.RoleId && !r.IsDeleted, cancellationToken);
            if (currentRole?.RoleType == RoleEnum.SuperAdministrator && !await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken))
            {
                throw new Exception("Super Administrator user cannot be deleted from User Management");
            }

            user.IsDeleted = true;
            user.DeletedBy = _currentUserService.UserId.ToString();
            user.DeletedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
