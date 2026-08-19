using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.StockBatches.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.StockBatches.Handlers
{
    public class DeleteStockBatchCommandHandler : IRequestHandler<DeleteStockBatchCommand>
    {
        private readonly IRepository<StockBatchEntity> _stockBatchRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteStockBatchCommandHandler(
            IRepository<StockBatchEntity> stockBatchRepository,
            IUnitOfWork unitOfWork)
        {
            _stockBatchRepository = stockBatchRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteStockBatchCommand request, CancellationToken cancellationToken)
        {
            var batch = await _stockBatchRepository.Query()
                .FirstOrDefaultAsync(x => x.StockBatchId == request.Id && !x.IsDeleted, cancellationToken);
            if (batch == null) throw new Exception("Batch not found");
            if (batch.RemainingQuantity < batch.Quantity)
                throw new Exception("Cannot delete batch that has been partially consumed");

            batch.IsDeleted = true;
            batch.DeletedAt = DateTime.UtcNow;
            batch.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}