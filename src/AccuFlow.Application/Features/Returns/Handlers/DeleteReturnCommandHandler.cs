using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Returns.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Handlers
{
    public class DeleteReturnCommandHandler : IRequestHandler<DeleteReturnCommand>
    {
        private readonly IRepository<GoodsReturnEntity> _goodsReturnRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReturnCommandHandler(
            IRepository<GoodsReturnEntity> goodsReturnRepository,
            IUnitOfWork unitOfWork)
        {
            _goodsReturnRepository = goodsReturnRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteReturnCommand request, CancellationToken cancellationToken)
        {
            var returnDoc = await _goodsReturnRepository.FirstOrDefaultAsync(
                x => x.GoodsReturnId == request.GoodsReturnId && !x.IsDeleted, cancellationToken);
            if (returnDoc == null) throw new Exception("Return not found");
            if (returnDoc.Status != "Draft") throw new Exception("Only draft return can be deleted");

            returnDoc.IsDeleted = true;
            returnDoc.DeletedAt = DateTime.UtcNow;
            returnDoc.DeletedBy = request.UserId.ToString();
            _goodsReturnRepository.Update(returnDoc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}