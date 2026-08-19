using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUnitCommandHandler(
            IRepository<UnitEntity> unitRepository,
            IUnitOfWork unitOfWork)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.Query()
                .FirstOrDefaultAsync(x => x.UnitId == request.Id && !x.IsDeleted, cancellationToken);
            if (unit == null) throw new Exception("Unit not found");
            unit.IsDeleted = true;
            unit.DeletedAt = DateTime.UtcNow;
            unit.DeletedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}