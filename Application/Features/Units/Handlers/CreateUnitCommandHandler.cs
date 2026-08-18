using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Units.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Units.Handlers
{
    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand>
    {
        private readonly IRepository<UnitEntity> _unitRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUnitCommandHandler(
            IRepository<UnitEntity> unitRepository,
            IUnitOfWork unitOfWork)
        {
            _unitRepository = unitRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            var exists = await _unitRepository.AnyAsync(x => x.UnitCode == request.Request.UnitCode && !x.IsDeleted, cancellationToken);
            if (exists) throw new Exception("Unit code already exists");
            _unitRepository.Add(new UnitEntity
            {
                UnitId = Guid.NewGuid(),
                UnitCode = request.Request.UnitCode,
                UnitName = request.Request.UnitName,
                Description = request.Request.Description,
                IsActive = true,
                CreatedBy = request.UserId.ToString()
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}