using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Roles.Commands;
using AccuFlow.Domain.Entities;
using AccuFlow.Entities.Enums;
using MediatR;

namespace AccuFlow.Application.Features.Roles.Handlers
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
    {
        private readonly IRepository<RoleEntity> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandler(
            IRepository<RoleEntity> roleRepository,
            IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var existingRole = await _roleRepository.FirstOrDefaultAsync(
                x => x.RoleType == (RoleEnum)request.Request.RoleType && !x.IsDeleted,
                cancellationToken);

            if (existingRole != null)
            {
                throw new Exception($"Role type '{((RoleEnum)request.Request.RoleType).ToString()}' already exists");
            }

            var role = new RoleEntity
            {
                RoleId = Guid.NewGuid(),
                RoleType = (RoleEnum)request.Request.RoleType,
                RoleName = request.Request.RoleName,
                Description = request.Request.Description ?? string.Empty,
                IsActive = request.Request.IsActive,
                Permissions = "[]"
            };

            role.CreatedBy = request.UserId.ToString();
            role.CreatedAt = DateTime.UtcNow;

            _roleRepository.Add(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return role.RoleId;
        }
    }
}