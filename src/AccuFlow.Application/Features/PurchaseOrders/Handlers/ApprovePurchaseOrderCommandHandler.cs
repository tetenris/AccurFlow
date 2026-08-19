using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.PurchaseOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.PurchaseOrders.Handlers
{
    public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand>
    {
        private readonly IRepository<PurchaseOrderEntity> _poRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApprovePurchaseOrderCommandHandler(
            IRepository<PurchaseOrderEntity> poRepository,
            IUnitOfWork unitOfWork)
        {
            _poRepository = poRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
        {
            var po = await _poRepository.Query()
                .FirstOrDefaultAsync(x => x.PurchaseOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (po == null) throw new Exception("Purchase order not found");
            if (po.Status != "Draft") throw new Exception("Only draft purchase order can be approved");
            po.Status = "Approved";
            po.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}