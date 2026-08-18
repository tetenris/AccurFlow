using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Account.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using System.Text.RegularExpressions;

namespace AccuFlow.Application.Features.Account.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        private const int PasswordExpiryDays = 30;

        public ChangePasswordCommandHandler(IRepository<UserEntity> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(
                x => x.UserId == request.UserId && !x.IsDeleted && x.IsActive,
                cancellationToken);

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw new Exception("User not found");
            }

            var isValidCurrentPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isValidCurrentPassword)
            {
                throw new Exception("Current password is incorrect");
            }

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            {
                throw new Exception("New password must be different from current password");
            }

            if (!Regex.IsMatch(request.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$"))
            {
                throw new Exception("New password must contain uppercase letter, lowercase letter, number, and symbol");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;
            user.PasswordExpiresAt = DateTime.UtcNow.AddDays(PasswordExpiryDays);
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedAt = null;
            user.LockedReason = null;
            user.UpdatedBy = request.UserId.ToString();
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}