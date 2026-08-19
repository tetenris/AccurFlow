using AccuFlow.Application.Common.Helpers;
using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Users.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Models.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Users.Handlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserViewModel?>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ISuperAdminCheck _superAdminCheck;

        public GetUserByIdQueryHandler(
            IRepository<UserEntity> userRepository,
            ISuperAdminCheck superAdminCheck)
        {
            _userRepository = userRepository;
            _superAdminCheck = superAdminCheck;
        }

        public async Task<UserViewModel?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var isSuperAdministrator = await _superAdminCheck.IsCurrentUserSuperAdministratorAsync(cancellationToken);

            var user = await _userRepository.Query()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.UserId == request.Id
                    && !u.IsDeleted
                    && (isSuperAdministrator || u.Role == null || u.Role.RoleType != RoleEnum.SuperAdministrator),
                    cancellationToken);

            if (user == null)
            {
                return null;
            }

            return new UserViewModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                FailedLoginAttempts = user.FailedLoginAttempts,
                PasswordExpiresAt = user.PasswordExpiresAt,
                LockedAt = user.LockedAt,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "",
                CreatedBy = user.CreatedBy,
                CreatedAt = user.CreatedAt,
                UpdatedBy = user.UpdatedBy,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}