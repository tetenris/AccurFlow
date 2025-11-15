param(
    [string]$TargetPath = "D:\ASP.NET\2025\AKURAT\AccuFlow"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Generating Role Module for AccuFlow" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Create directories
$directories = @(
    "Entities",
    "Controllers",
    "Services",
    "Models\Role",
    "Views\Role",
    "wwwroot\custom\features\role"
)

Write-Host "Creating directories..." -ForegroundColor Yellow
foreach ($dir in $directories) {
    $fullPath = Join-Path $TargetPath $dir
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
        Write-Host "  Created: $dir" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Generating files..." -ForegroundColor Yellow

# ============================================
# 1. RoleEntity.cs
# ============================================
$roleEntity = @'
using System.ComponentModel.DataAnnotations;

namespace AccuFlow.Entities
{
    public class RoleEntity
    {
        [Key]
        public Guid RoleId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Entities\RoleEntity.cs") -Value $roleEntity
Write-Host "  Generated: Entities\RoleEntity.cs" -ForegroundColor Green

# ============================================
# 2. DataTableRoleRequest.cs
# ============================================
$roleRequest = @'
using AccuFlow.Models.BaseModel;

namespace AccuFlow.Models.Role
{
    public class DataTableRoleRequest : BaseDatatableRequest
    {
        public bool? IsActive { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Models\Role\DataTableRoleRequest.cs") -Value $roleRequest
Write-Host "  Generated: Models\Role\DataTableRoleRequest.cs" -ForegroundColor Green

# ============================================
# 3. RoleViewModel.cs
# ============================================
$roleViewModel = @'
namespace AccuFlow.Models.Role
{
    public class RoleViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class EditRoleRequest
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Models\Role\RoleViewModel.cs") -Value $roleViewModel
Write-Host "  Generated: Models\Role\RoleViewModel.cs" -ForegroundColor Green

# ============================================
# 4. RoleService.cs
# ============================================
$roleService = @'
using AccuFlow.Data;
using AccuFlow.Entities;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.Role;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace AccuFlow.Services
{
    public interface IRoleService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request);
        Task<RoleViewModel?> GetById(Guid roleId);
        Task Create(CreateRoleRequest request, Guid userId);
        Task Edit(EditRoleRequest request, Guid userId);
        Task Delete(Guid roleId);
    }

    public class RoleService : BaseService, IRoleService
    {
        public RoleService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request)
        {
            var query = _dbContext.Set<RoleEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            var totalRecord = await query.CountAsync();

            // Search
            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x => 
                    x.RoleName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search))
                );
            }

            // Filter by IsActive
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            // Sorting
            query = request?.OrderBy?.ToLower() switch
            {
                "rolename" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.RoleName) 
                    : query.OrderByDescending(x => x.RoleName),
                "isactive" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.IsActive) 
                    : query.OrderByDescending(x => x.IsActive),
                "createdat" => request?.OrderType?.ToLower() == "asc" 
                    ? query.OrderBy(x => x.CreatedAt) 
                    : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var totalFiltered = await query.CountAsync();

            // Paging
            var data = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy.ToString(),
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy.ToString(),
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return new BaseDatatableResponse
            {
                Draw = request.Draw,
                RecordsTotal = totalRecord,
                RecordsFiltered = totalFiltered,
                Data = data
            };
        }

        public async Task<RoleViewModel?> GetById(Guid roleId)
        {
            return await _dbContext.Set<RoleEntity>()
                .Where(x => x.RoleId == roleId && !x.IsDeleted)
                .Select(x => new RoleViewModel
                {
                    RoleId = x.RoleId,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy.ToString(),
                    CreatedAt = x.CreatedAt,
                    UpdatedBy = x.UpdatedBy.ToString(),
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task Create(CreateRoleRequest request, Guid userId)
        {
            var role = new RoleEntity
            {
                RoleId = Guid.NewGuid(),
                RoleName = request.RoleName,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Set<RoleEntity>().Add(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(EditRoleRequest request, Guid userId)
        {
            var role = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && !x.IsDeleted);

            if (role == null)
                throw new Exception("Role not found");

            role.RoleName = request.RoleName;
            role.Description = request.Description;
            role.IsActive = request.IsActive;
            role.UpdatedBy = userId;
            role.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid roleId)
        {
            var role = await _dbContext.Set<RoleEntity>()
                .FirstOrDefaultAsync(x => x.RoleId == roleId && !x.IsDeleted);

            if (role == null)
                throw new Exception("Role not found");

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Services\RoleService.cs") -Value $roleService
Write-Host "  Generated: Services\RoleService.cs" -ForegroundColor Green

# ============================================
# 5. RoleController.cs
# ============================================
$roleController = @'
using AccuFlow.Models.Role;
using AccuFlow.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccuFlow.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService) : base(roleService)
        {
            _roleService = roleService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Role Management";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Datatable([FromBody] DataTableRoleRequest request)
        {
            var result = await _roleService.Datatable(request);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _roleService.GetById(id);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            try
            {
                await _roleService.Create(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Role created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] EditRoleRequest request)
        {
            try
            {
                await _roleService.Edit(request, _currentUserService.UserId);
                return Ok(new { success = true, message = "Role updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Guid id)
        {
            try
            {
                await _roleService.Delete(id);
                return Ok(new { success = true, message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
'@
Set-Content -Path (Join-Path $TargetPath "Controllers\RoleController.cs") -Value $roleController
Write-Host "  Generated: Controllers\RoleController.cs" -ForegroundColor Green

# ============================================
# 6. Index.cshtml
# ============================================
$indexView = @'
@{
    ViewData["Title"] = "Role Management";
}

<div class="card">
    <div class="card-header border-0 pt-5">
        <h3 class="card-title align-items-start flex-column">
            <span class="card-label fw-bold fs-3 mb-1">Role Management</span>
        </h3>
        <div class="card-toolbar">
            <button type="button" class="btn btn-primary" id="btn-add-role">
                <i class="fa fa-plus"></i> Add Role
            </button>
        </div>
    </div>
    <div class="card-body">
        <div class="row mb-5">
            <div class="col-md-3">
                <label>Status</label>
                <select class="form-select" id="filter-status">
                    <option value="">All Status</option>
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                </select>
            </div>
            <div class="col-md-3 d-flex align-items-end">
                <button class="btn btn-primary" id="btn-apply-filter">Apply Filter</button>
            </div>
        </div>
        <div class="table-responsive">
            <table id="role_datatable" class="table table-striped table-row-bordered gy-5 gs-7 border rounded w-100">
                <thead>
                    <tr>
                        <th>No</th>
                        <th>Role Name</th>
                        <th>Description</th>
                        <th>Status</th>
                        <th>Created At</th>
                        <th>Action</th>
                    </tr>
                </thead>
            </table>
        </div>
    </div>
</div>

<!-- Modal Add/Edit -->
<div class="modal fade" id="modal-role" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modal-role-title">Add Role</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="form-role">
                    <input type="hidden" id="role-id" />
                    <div class="mb-3">
                        <label class="form-label required">Role Name</label>
                        <input type="text" class="form-control" id="role-name" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <textarea class="form-control" id="role-description" rows="3"></textarea>
                    </div>
                    <div class="mb-3">
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" id="role-active" checked />
                            <label class="form-check-label" for="role-active">Active</label>
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" id="btn-save-role">Save</button>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/custom/features/role/index.js"></script>
}
'@
Set-Content -Path (Join-Path $TargetPath "Views\Role\Index.cshtml") -Value $indexView
Write-Host "  Generated: Views\Role\Index.cshtml" -ForegroundColor Green

# ============================================
# 7. index.js
# ============================================
$indexJs = @'
$(document).ready(function () {
    let isEditMode = false;
    let currentRoleId = null;

    // Initialize DataTable
    const table = $("#role_datatable").DataTable({
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search',
            emptyTable: "No data available",
            infoEmpty: "No entries to show"
        },
        ajax: {
            url: '/Role/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const isActive = $('#filter-status').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    IsActive: isActive === "" ? null : isActive === "true"
                });
            }
        },
        order: [[4, 'desc']],
        columns: [
            {
                data: null,
                orderable: false,
                render: function (data, type, row, meta) {
                    return meta.settings._iDisplayStart + meta.row + 1;
                }
            },
            { data: "roleName" },
            { data: "description", defaultContent: "-" },
            {
                data: "isActive",
                render: function (data) {
                    return data 
                        ? '<span class="badge badge-light-success">Active</span>' 
                        : '<span class="badge badge-light-danger">Inactive</span>';
                }
            },
            {
                data: "createdAt",
                render: function (data) {
                    return moment(data).format("DD-MMM-YYYY HH:mm");
                }
            },
            {
                data: "roleId",
                orderable: false,
                render: function (data) {
                    return `
                        <button class="btn btn-sm btn-icon btn-primary btn-edit" data-id="${data}">
                            <i class="fa fa-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-icon btn-danger btn-delete" data-id="${data}">
                            <i class="fa fa-trash"></i>
                        </button>
                    `;
                }
            }
        ]
    });

    // Apply Filter
    $('#btn-apply-filter').on('click', function () {
        table.ajax.reload();
    });

    // Add Role
    $('#btn-add-role').on('click', function () {
        isEditMode = false;
        currentRoleId = null;
        $('#modal-role-title').text('Add Role');
        $('#form-role')[0].reset();
        $('#role-active').prop('checked', true);
        $('#modal-role').modal('show');
    });

    // Edit Role
    $('#role_datatable').on('click', '.btn-edit', async function () {
        const roleId = $(this).data('id');
        isEditMode = true;
        currentRoleId = roleId;
        
        try {
            const response = await $.get(`/Role/GetById?id=${roleId}`);
            $('#modal-role-title').text('Edit Role');
            $('#role-id').val(response.roleId);
            $('#role-name').val(response.roleName);
            $('#role-description').val(response.description);
            $('#role-active').prop('checked', response.isActive);
            $('#modal-role').modal('show');
        } catch (error) {
            Swal.fire('Error', 'Failed to load role data', 'error');
        }
    });

    // Save Role
    $('#btn-save-role').on('click', async function () {
        const roleName = $('#role-name').val().trim();
        if (!roleName) {
            Swal.fire('Warning', 'Role name is required', 'warning');
            return;
        }

        const data = {
            RoleName: roleName,
            Description: $('#role-description').val(),
            IsActive: $('#role-active').is(':checked')
        };

        if (isEditMode) {
            data.RoleId = currentRoleId;
        }

        try {
            const url = isEditMode ? '/Role/Edit' : '/Role/Create';
            const response = await $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            });

            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-role').modal('hide');
                table.ajax.reload();
            }
        } catch (error) {
            Swal.fire('Error', error.responseJSON?.message || 'Failed to save role', 'error');
        }
    });

    // Delete Role
    $('#role_datatable').on('click', '.btn-delete', function () {
        const roleId = $(this).data('id');
        
        Swal.fire({
            title: 'Delete Role?',
            text: "This action cannot be undone",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn btn-danger',
                cancelButton: 'btn btn-secondary'
            },
            buttonsStyling: false
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    const response = await $.ajax({
                        url: '/Role/Delete',
                        type: 'DELETE',
                        contentType: 'application/json',
                        data: JSON.stringify(roleId)
                    });

                    if (response.success) {
                        Swal.fire('Deleted!', response.message, 'success');
                        table.ajax.reload();
                    }
                } catch (error) {
                    Swal.fire('Error', error.responseJSON?.message || 'Failed to delete role', 'error');
                }
            }
        });
    });
});
'@
Set-Content -Path (Join-Path $TargetPath "wwwroot\custom\features\role\index.js") -Value $indexJs
Write-Host "  Generated: wwwroot\custom\features\role\index.js" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Role Module Generated Successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files generated:" -ForegroundColor Yellow
Write-Host "  - Entities\RoleEntity.cs" -ForegroundColor White
Write-Host "  - Models\Role\DataTableRoleRequest.cs" -ForegroundColor White
Write-Host "  - Models\Role\RoleViewModel.cs" -ForegroundColor White
Write-Host "  - Services\RoleService.cs" -ForegroundColor White
Write-Host "  - Controllers\RoleController.cs" -ForegroundColor White
Write-Host "  - Views\Role\Index.cshtml" -ForegroundColor White
Write-Host "  - wwwroot\custom\features\role\index.js" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Update AppDbContext.cs to add DbSet<RoleEntity>" -ForegroundColor White
Write-Host "  2. Register IRoleService in Program.cs" -ForegroundColor White
Write-Host "  3. Run migration: dotnet ef migrations add AddRoleEntity" -ForegroundColor White
Write-Host "  4. Update database: dotnet ef database update" -ForegroundColor White
Write-Host ""
