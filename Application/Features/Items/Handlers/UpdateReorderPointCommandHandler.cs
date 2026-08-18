using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class UpdateReorderPointCommandHandler : IRequestHandler<UpdateReorderPointCommand>
    {
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateReorderPointCommandHandler(
            IRepository<ItemEntity> itemRepository,
            IUnitOfWork unitOfWork)
        {
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateReorderPointCommand request, CancellationToken cancellationToken)
        {
            var item = await _itemRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemId == request.Request.ItemId && !x.IsDeleted, cancellationToken);
            if (item == null) throw new Exception("Item not found");
            if (request.Request.ReorderPoint < 0) throw new Exception("Reorder point cannot be negative");
            item.ReorderPoint = request.Request.ReorderPoint;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}