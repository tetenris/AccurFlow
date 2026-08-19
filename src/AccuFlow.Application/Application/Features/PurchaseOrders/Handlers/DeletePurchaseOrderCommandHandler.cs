using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class DeletePurchaseOrderCommandHandler : IRequestHandler<DeletePurchaseOrderCommand>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePurchaseOrderCommandHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var po = await _poRepository.Query()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be deleted");
            po.IsDeleted = true;
            po.DeletedBy = request.UserId.ToString();
            po.DeletedAt = DateTime.UtcNow;
            foreach (var line in po.Lines)
            {
                line.IsDeleted = true;
                line.DeletedBy = request.UserId.ToString();
                line.DeletedAt = DateTime.UtcNow;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}