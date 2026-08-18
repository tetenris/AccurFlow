using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class CreateItemGroupCommandHandler : IRequestHandler<CreateItemGroupCommand>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateItemGroupCommandHandler(
            IRepository<ItemGroupEntity> itemGroupRepository,
            IUnitOfWork unitOfWork)
        {
            _itemGroupRepository = itemGroupRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateItemGroupCommand request, CancellationToken cancellationToken)
        {
            var exists = await _itemGroupRepository.AnyAsync(x => x.GroupCode == request.Request.GroupCode && !x.IsDeleted, cancellationToken);
            if (exists) throw new Exception("Group code already exists");
            _itemGroupRepository.Add(new ItemGroupEntity
            {
                ItemGroupId = Guid.NewGuid(),
                GroupCode = request.Request.GroupCode,
                GroupName = request.Request.GroupName,
                Description = request.Request.Description,
                IsActive = true,
                CreatedBy = request.UserId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}