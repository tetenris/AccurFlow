using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.GoodsReceipts.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.GoodsReceipts.Handlers
{
    public class DeleteGoodsReceiptCommandHandler : IRequestHandler<DeleteGoodsReceiptCommand>
    {
        private readonly IRepository<GoodsReceiptEntity> _grnRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteGoodsReceiptCommandHandler(
            IRepository<GoodsReceiptEntity> grnRepository,
            IUnitOfWork unitOfWork)
        {
            _grnRepository = grnRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteGoodsReceiptCommand request, CancellationToken cancellationToken)
        {
            var grn = await _grnRepository.Query()
                .FirstOrDefaultAsync(x => x.GoodsReceiptId == request.Id && !x.IsDeleted, cancellationToken);
            if (grn == null) throw new Exception("Goods receipt not found");
            if (grn.Status != "Draft") throw new Exception("Only draft goods receipt can be deleted");

            grn.IsDeleted = true;
            grn.DeletedAt = DateTime.UtcNow;
            grn.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}