using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Account.Commands;
using AccuFlow.Application.Features.Account.Dtos;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Account.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        private const int MaxFailedLoginAttempts = 3;

        public LoginCommandHandler(IRepository<UserEntity> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var normalizedUsername = request.Username.Trim().ToLower();
            var user = await _userRepository.FirstOrDefaultWithIncludesAsync(
                x => !x.IsDeleted
                    && x.IsActive
                    && (x.UserName.ToLower() == normalizedUsername || x.Email.ToLower() == normalizedUsername),
                cancellationToken,
                "Role");

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Failed("Invalid username or password");
            }

            if (user.IsLocked)
            {
                return Failed("Password anda terkunci. Silakan hubungi admin.", true);
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                user.FailedLoginAttempts += 1;
                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.IsLocked = true;
                    user.LockedAt = DateTime.UtcNow;
                    user.LockedReason = "Password salah 3 kali berturut-turut";
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Failed("Password anda terkunci. Silakan hubungi admin.", true);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var remainingAttempts = MaxFailedLoginAttempts - user.FailedLoginAttempts;
                return Failed($"Invalid username or password. Sisa percobaan: {remainingAttempts}");
            }

            if (user.FailedLoginAttempts > 0 || user.LockedAt != null || !string.IsNullOrWhiteSpace(user.LockedReason))
            {
                user.FailedLoginAttempts = 0;
                user.LockedAt = null;
                user.LockedReason = null;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new LoginResultDto
            {
                Succeeded = true,
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                PasswordExpiresAt = user.PasswordExpiresAt
            };
        }

        private static LoginResultDto Failed(string message, bool isLocked = false)
        {
            return new LoginResultDto
            {
                Succeeded = false,
                IsLocked = isLocked,
                Message = message
            };
        }
    }
}