using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.ProductionOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.ProductionOrders.Handlers
{
    public class DeleteProductionOrderCommandHandler : IRequestHandler<DeleteProductionOrderCommand>
    {
        private readonly IRepository<ProductionOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductionOrderCommandHandler(
            IRepository<ProductionOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteProductionOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .FirstOrDefaultAsync(x => x.ProductionOrderId == request.ProductionOrderId && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Production order not found");
            if (order.Status != "Draft") throw new Exception("Only draft production order can be deleted");

            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;
            order.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}