using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ItemGroups.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ItemGroups.Handlers
{
    public class DeleteItemGroupCommandHandler : IRequestHandler<DeleteItemGroupCommand>
    {
        private readonly IRepository<ItemGroupEntity> _itemGroupRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteItemGroupCommandHandler(
            IRepository<ItemGroupEntity> itemGroupRepository,
            IRepository<ItemEntity> itemRepository,
            IUnitOfWork unitOfWork)
        {
            _itemGroupRepository = itemGroupRepository;
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteItemGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _itemGroupRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemGroupId == request.Id && !x.IsDeleted, cancellationToken);
            if (group == null) throw new Exception("Item group not found");
            if (await _itemRepository.AnyAsync(x => x.ItemGroupId == request.Id && !x.IsDeleted, cancellationToken))
                throw new Exception("Item group is used by items and cannot be deleted");
            group.IsDeleted = true;
            group.DeletedAt = DateTime.UtcNow;
            group.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}