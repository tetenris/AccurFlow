using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUnitCommandHandler(
            IRepository<UnitEntity> unitRepository,
            IUnitOfWork unitOfWork)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.Query()
                .FirstOrDefaultAsync(x => x.UnitId == request.Request.UnitId && !x.IsDeleted, cancellationToken);
            if (unit == null) throw new Exception("Unit not found");
            unit.UnitCode = request.Request.UnitCode;
            unit.UnitName = request.Request.UnitName;
            unit.Description = request.Request.Description;
            unit.IsActive = request.Request.IsActive;
            unit.UpdatedAt = DateTime.UtcNow;
            unit.UpdatedBy = request.UserId.ToString();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}