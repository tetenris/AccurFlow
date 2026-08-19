using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class UpdateItemGroupCommandHandler : IRequestHandler<UpdateItemGroupCommand>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateItemGroupCommandHandler(
            IRepository<ItemGroupEntity> itemGroupRepository,
            IUnitOfWork unitOfWork)
        {
            _itemGroupRepository = itemGroupRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateItemGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _itemGroupRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemGroupId == request.Request.ItemGroupId && !x.IsDeleted, cancellationToken);
            if (group == null) throw new Exception("Item group not found");
            group.GroupCode = request.Request.GroupCode;
            group.GroupName = request.Request.GroupName;
            group.Description = request.Request.Description;
            group.IsActive = request.Request.IsActive;
            group.UpdatedAt = DateTime.UtcNow;
            group.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}