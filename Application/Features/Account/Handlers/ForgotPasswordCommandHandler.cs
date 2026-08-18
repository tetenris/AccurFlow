using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Account.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Account.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IRepository<UserEntity> _userRepository;

        public ForgotPasswordCommandHandler(IRepository<UserEntity> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.Trim().ToLower();
            var user = await _userRepository.FirstOrDefaultAsync(
                x => !x.IsDeleted && x.IsActive && x.Email.ToLower() == normalizedEmail,
                cancellationToken);

            if (user == null)
            {
                return false;
            }

            // Email delivery is not configured yet. This handler is the integration point for future email reset links.
            return true;
        }
    }
}