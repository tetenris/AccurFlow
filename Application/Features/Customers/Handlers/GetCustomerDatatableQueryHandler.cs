using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.Customers.Queries;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Customer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Application.Features.Customers.Handlers
{
    public class GetCustomerDatatableQueryHandler : IRequestHandler<GetCustomerDatatableQuery, BaseDatatableResponse>
    {
        private readonly IRepository<CustomerEntity> _customerRepository;
        private readonly IRepository<UserEntity> _userRepository;

        public GetCustomerDatatableQueryHandler(
            IRepository<CustomerEntity> customerRepository,
            IRepository<UserEntity> userRepository)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseDatatableResponse> Handle(GetCustomerDatatableQuery request, CancellationToken cancellationToken)
        {
            var r = request.Request;

            IQueryable<CustomerEntity> query = _customerRepository.Query()
                .Where(x => !x.IsDeleted);

            var totalRecord = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(r.Search))
            {
                var search = r.Search.ToLower();
                query = query.Where(x =>
                    x.CustomerCode.ToLower().Contains(search) ||
                    x.CustomerName.ToLower().Contains(search) ||
                    (x.ContactPerson != null && x.ContactPerson.ToLower().Contains(search)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(search)) ||
                    (x.Email != null && x.Email.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(r.CustomerType))
            {
                query = query.Where(x => x.CustomerType == r.CustomerType);
            }

            if (r.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == r.IsActive.Value);
            }

            query = r.OrderBy?.ToLower() switch
            {
                "customercode" => r.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerCode)
                    : query.OrderByDescending(x => x.CustomerCode),
                "customername" => r.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerName)
                    : query.OrderByDescending(x => x.CustomerName),
                "customertype" => r.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CustomerType)
                    : query.OrderByDescending(x => x.CustomerType),
                "createdat" => r.OrderType?.ToLower() == "asc"
                    ? query.OrderBy(x => x.CreatedAt)
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderBy(x => x.CustomerCode)
            };

            var totalFiltered = await query.CountAsync(cancellationToken);

            var data = await query
                .Skip((r.Page - 1) * r.Size)
                .Take(r.Size)
                .Select(x => new CustomerViewModel
                {
                    CustomerId = x.CustomerId,
                    CustomerCode = x.CustomerCode,
                    CustomerName = x.CustomerName,
                    CustomerType = x.CustomerType,
                    ContactPerson = x.ContactPerson,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Address = x.Address,
                    City = x.City,
                    State = x.State,
                    PostalCode = x.PostalCode,
                    Country = x.Country,
                    CreditLimit = x.CreditLimit,
                    PaymentTerms = x.PaymentTerms,
                    CurrentBalance = x.CurrentBalance,
                    TaxId = x.TaxId,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var userIds = data
                .SelectMany(x => new[] { x.CreatedBy, x.UpdatedBy })
                .Where(id => !string.IsNullOrEmpty(id) && Guid.TryParse(id, out _))
                .Distinct()
                .Select(id => Guid.Parse(id!))
                .ToList();

            var users = await _userRepository.Query()
                .Where(u => userIds.Contains(u.UserId))
                .Select(u => new { u.UserId, Name = u.FullName ?? u.UserName })
                .ToDictionaryAsync(u => u.UserId.ToString(), u => u.Name, cancellationToken);

            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.CreatedBy) && users.TryGetValue(item.CreatedBy, out var createdByName))
                {
                    item.CreatedBy = createdByName;
                }
                else
                {
                    item.CreatedBy = "System";
                }

                if (!string.IsNullOrEmpty(item.UpdatedBy) && users.TryGetValue(item.UpdatedBy, out var updatedByName))
                {
                    item.UpdatedBy = updatedByName;
                }
            }

            return new BaseDatatableResponse
            {
                Draw = r.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }
    }
}