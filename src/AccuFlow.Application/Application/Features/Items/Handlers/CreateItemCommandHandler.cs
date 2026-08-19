using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Items.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Items.Handlers
{
    public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand>
    {
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateItemCommandHandler(
            IRepository<ItemEntity> itemRepository,
            IUnitOfWork unitOfWork)
        {
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateItemCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            var exists = await _itemRepository.AnyAsync(x => x.ItemCode == r.ItemCode && !x.IsDeleted, cancellationToken);
            if (exists) throw new Exception("Item code already exists");
            _itemRepository.Add(new ItemEntity
            {
                ItemId = Guid.NewGuid(),
                ItemCode = r.ItemCode,
                ItemName = r.ItemName,
                ItemType = r.ItemType,
                ItemGroupId = r.ItemGroupId,
                Unit = r.Unit,
                SalesPrice = r.SalesPrice,
                PurchasePrice = r.PurchasePrice,
                ReorderPoint = r.ReorderPoint,
                InventoryAccountId = r.InventoryAccountId,
                SalesAccountId = r.SalesAccountId,
                CostOfGoodsSoldAccountId = r.CostOfGoodsSoldAccountId,
                Description = r.Description,
                IsActive = true,
                CreatedBy = request.UserId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}