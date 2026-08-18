using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Returns.Helpers
{
    public static class ReturnHelper
    {
        public static async Task<Dictionary<Guid, decimal>> GetReturnedByInvoiceLineAsync(
            IRepository<GoodsReturnLineEntity> goodsReturnLineRepository,
            CancellationToken cancellationToken = default)
        {
            return await goodsReturnLineRepository.Query()
                .Where(x => !x.IsDeleted && x.GoodsReturn != null && !x.GoodsReturn.IsDeleted && x.GoodsReturn.Status == "Posted")
                .GroupBy(x => x.InvoiceLineId)
                .Select(g => new { InvoiceLineId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.InvoiceLineId, x => x.Quantity, cancellationToken);
        }
    }
}