using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DeliveryOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.DeliveryOrders.Handlers
{
    public class DeleteDeliveryCommandHandler : IRequestHandler<DeleteDeliveryCommand>
    {
        private readonly IRepository<DeliveryOrderEntity> _deliveryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDeliveryCommandHandler(
            IRepository<DeliveryOrderEntity> deliveryRepository,
            IUnitOfWork unitOfWork)
        {
            _deliveryRepository = deliveryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteDeliveryCommand request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.Query()
                .FirstOrDefaultAsync(x => x.DeliveryOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (delivery == null) throw new Exception("Delivery order not found");
            if (delivery.Status != "Draft") throw new Exception("Only draft delivery order can be deleted");
            delivery.IsDeleted = true; delivery.DeletedAt = DateTime.UtcNow; delivery.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}