using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Commands;
using AccuFlow.Domain.Entities;
using MediatR;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class SaveRoleMenuPermissionsCommandHandler : IRequestHandler<SaveRoleMenuPermissionsCommand>
    {
        private readonly IRepository<RoleMenuEntity> _roleMenuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SaveRoleMenuPermissionsCommandHandler(
            IRepository<RoleMenuEntity> roleMenuRepository,
            IUnitOfWork unitOfWork)
        {
            _roleMenuRepository = roleMenuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SaveRoleMenuPermissionsCommand request, CancellationToken cancellationToken)
        {
            var existingPermissions = await _roleMenuRepository.FindAsync(
                rm => rm.RoleId == request.Request.RoleId,
                cancellationToken);

            _roleMenuRepository.RemoveRange(existingPermissions);

            foreach (var permission in request.Request.Permissions)
            {
                _roleMenuRepository.Add(new RoleMenuEntity
                {
                    RoleId = request.Request.RoleId,
                    MenuId = permission.MenuId,
                    CanView = permission.CanView,
                    CanAdd = permission.CanAdd,
                    CanEdit = permission.CanEdit,
                    CanDelete = permission.CanDelete,
                    CanPost = permission.CanPost,
                    CanReverse = permission.CanReverse
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}