using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockBatches.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockBatches.Handlers
{
    public class CreateStockBatchCommandHandler : IRequestHandler<CreateStockBatchCommand>
    {
        private readonly IRepository<StockBatchEntity> _stockBatchRepository;
        private readonly IRepository<ItemEntity> _itemRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStockBatchCommandHandler(
            IRepository<StockBatchEntity> stockBatchRepository,
            IRepository<ItemEntity> itemRepository,
            IUnitOfWork unitOfWork)
        {
            _stockBatchRepository = stockBatchRepository;
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateStockBatchCommand request, CancellationToken cancellationToken)
        {
            var r = request.Request;
            if (r.ItemId == Guid.Empty) throw new Exception("Item is required");
            if (string.IsNullOrWhiteSpace(r.BatchNumber)) throw new Exception("Batch/serial number is required");
            if (r.Quantity <= 0) throw new Exception("Quantity must be greater than zero");

            var item = await _itemRepository.Query()
                .FirstOrDefaultAsync(x => x.ItemId == r.ItemId && !x.IsDeleted, cancellationToken);
            if (item == null) throw new Exception("Item not found");

            var batch = new StockBatchEntity
            {
                StockBatchId = Guid.NewGuid(),
                ItemId = r.ItemId,
                BatchNumber = r.BatchNumber.Trim(),
                Quantity = r.Quantity,
                RemainingQuantity = r.Quantity,
                ExpiryDate = r.ExpiryDate,
                Notes = r.Notes,
                IsActive = true,
                CreatedBy = request.UserId.ToString()
            };

            _stockBatchRepository.Add(batch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}