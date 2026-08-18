using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrderCommandHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .FirstOrDefaultAsync(x => x.SalesOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be deleted");
            order.IsDeleted = true; order.DeletedAt = DateTime.UtcNow; order.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}