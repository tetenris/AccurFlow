using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.SalesOrders.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.SalesOrders.Handlers
{
    public class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand>
    {
        private readonly IRepository<SalesOrderEntity> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveOrderCommandHandler(
            IRepository<SalesOrderEntity> orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .FirstOrDefaultAsync(x => x.SalesOrderId == request.Id && !x.IsDeleted, cancellationToken);
            if (order == null) throw new Exception("Sales order not found");
            if (order.Status != "Draft") throw new Exception("Only draft sales order can be approved");
            order.Status = "Approved";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}