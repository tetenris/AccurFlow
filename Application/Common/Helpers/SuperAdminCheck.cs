using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using AccuFlow.Infrastructures;

namespace AccuFlow.Application.Common.Helpers
{
    public interface ISuperAdminCheck
    {
        Task<bool> IsCurrentUserSuperAdministratorAsync(CancellationToken cancellationToken = default);
    }

    public class SuperAdminCheck : ISuperAdminCheck
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<RoleEntity> _roleRepository;

        public SuperAdminCheck(ICurrentUserService currentUserService, IRepository<RoleEntity> roleRepository)
        {
            _currentUserService = currentUserService;
            _roleRepository = roleRepository;
        }

        public async Task<bool> IsCurrentUserSuperAdministratorAsync(CancellationToken cancellationToken = default)
        {
            if (_currentUserService.RoleId == Guid.Empty)
            {
                return false;
            }

            return await _roleRepository.AnyAsync(
                x => x.RoleId == _currentUserService.RoleId
                    && x.RoleType == RoleEnum.SuperAdministrator
                    && x.IsActive
                    && !x.IsDeleted,
                cancellationToken);
        }
    }
}