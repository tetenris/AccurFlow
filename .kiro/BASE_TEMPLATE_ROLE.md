# Base Template: Role Management

## Overview
Role Management dijadikan sebagai **base template** untuk semua master data lainnya (Customers, Suppliers, Products, Tax, dll). Template ini lebih sederhana dan mudah diikuti dibandingkan Chart of Accounts yang lebih kompleks.

---

## 📁 File Structure

```
Role Management/
├── Entities/
│   ├── Entity/
│   │   └── RoleEntity.cs                    ← Entity dengan BaseEntity
│   └── EntityConfigurations/
│       └── RoleEntityConfiguration.cs       ← EF Core Configuration
├── Models/
│   └── Role/
│       ├── RoleViewModel.cs                 ← Display model
│       ├── CreateRoleRequest.cs             ← Create DTO dengan validasi
│       ├── EditRoleRequest.cs               ← Update DTO
│       └── DataTableRoleRequest.cs          ← DataTable filter
├── Services/
│   └── RoleService.cs                       ← Service + Interface dalam 1 file
├── Controllers/
│   └── RoleController.cs                    ← Controller dengan endpoints
├── Views/
│   └── Role/
│       └── Index.cshtml                     ← View dengan modal
└── wwwroot/custom/features/role/
    └── index.js                             ← JavaScript DataTable & CRUD
```

---

## 🏗️ Pattern & Conventions

### 1. Entity Pattern
```csharp
public class RoleEntity : BaseEntity
{
    public Guid RoleId { get; set; }           // Primary Key: {Entity}Id
    public string RoleName { get; set; }       // Required field
    public string? Description { get; set; }   // Optional field (nullable)
    public bool IsActive { get; set; }         // Status flag
    
    // BaseEntity provides:
    // - CreatedBy, CreatedAt
    // - UpdatedBy, UpdatedAt
    // - IsDeleted, DeletedAt, DeletedBy
}
```

**Key Points:**
- Extend `BaseEntity` untuk audit fields
- Primary Key: `{Entity}Id` (Guid)
- Required fields: non-nullable
- Optional fields: nullable dengan `?`
- Always include `IsActive` untuk status

### 2. Entity Configuration Pattern
```csharp
public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(e => e.RoleId);
        
        // Required fields
        builder.Property(e => e.RoleName).IsRequired().HasMaxLength(255);
        
        // Optional fields
        builder.Property(e => e.Description).HasMaxLength(500);
        
        // Default values
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        
        // Indexes
        builder.HasIndex(e => e.RoleName).IsUnique();
        builder.HasIndex(e => e.IsActive);
    }
}
```

**Key Points:**
- Table name: Plural form
- Set max length untuk string fields
- Add unique index untuk code/name fields
- Add index untuk IsActive (untuk filtering)

### 3. Service Pattern (Interface + Implementation dalam 1 file)
```csharp
// Interface
public interface IRoleService : IBaseService
{
    Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request);
    Task<RoleViewModel?> GetById(Guid roleId);
    Task Create(CreateRoleRequest request, Guid userId);
    Task Edit(EditRoleRequest request, Guid userId);
    Task Delete(Guid roleId);
    // Additional methods as needed
}

// Implementation
public class RoleService : BaseService, IRoleService
{
    public RoleService(AppDbContext dbContext) : base(dbContext) { }
    
    // Implement all interface methods
}
```

**Key Points:**
- Interface dan Implementation dalam 1 file
- Interface extends `IBaseService`
- Implementation extends `BaseService`
- Constructor inject `AppDbContext`
- All methods async dengan `Task` atau `Task<T>`

### 4. CRUD Methods Pattern

#### Create
```csharp
public async Task Create(CreateRoleRequest request, Guid userId)
{
    // 1. Validation (uniqueness, business rules)
    var existing = await _dbContext.Set<RoleEntity>()
        .FirstOrDefaultAsync(x => x.RoleName == request.RoleName && !x.IsDeleted);
    if (existing != null)
        throw new Exception("Role already exists");
    
    // 2. Get user name for audit trail
    var user = await _dbContext.Set<UserEntity>()
        .Where(u => u.UserId == userId)
        .Select(u => u.FullName ?? u.UserName)
        .FirstOrDefaultAsync();
    
    // 3. Create entity
    var entity = new RoleEntity
    {
        RoleId = Guid.NewGuid(),
        RoleName = request.RoleName,
        Description = request.Description,
        IsActive = request.IsActive
    };
    
    // 4. Set audit fields with user name (not ID)
    entity.CreatedBy = user ?? "System";
    entity.CreatedAt = DateTime.UtcNow;
    
    // 5. Save
    _dbContext.Set<RoleEntity>().Add(entity);
    await _dbContext.SaveChangesAsync();
}
```

**Important Notes:**
- ✅ Store **user name** (FullName or UserName) in audit fields, NOT user ID
- ✅ This makes audit trail readable without JOIN queries
- ✅ Use `FullName ?? UserName` as fallback
- ✅ Use "System" as default if user not found

#### Read (GetById)
```csharp
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
            CreatedBy = x.CreatedBy,
            CreatedAt = x.CreatedAt,
            UpdatedBy = x.UpdatedBy,
            UpdatedAt = x.UpdatedAt
        })
        .FirstOrDefaultAsync();
}
```

#### Update
```csharp
public async Task Edit(EditRoleRequest request, Guid userId)
{
    // 1. Find entity
    var entity = await _dbContext.Set<RoleEntity>()
        .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && !x.IsDeleted);
    if (entity == null)
        throw new Exception("Role not found");
    
    // 2. Validation (if needed)
    
    // 3. Get user name for audit trail
    var user = await _dbContext.Set<UserEntity>()
        .Where(u => u.UserId == userId)
        .Select(u => u.FullName ?? u.UserName)
        .FirstOrDefaultAsync();
    
    // 4. Update properties
    entity.RoleName = request.RoleName;
    entity.Description = request.Description;
    entity.IsActive = request.IsActive;
    
    // 5. Set audit fields with user name (not ID)
    entity.UpdatedBy = user ?? "System";
    entity.UpdatedAt = DateTime.UtcNow;
    
    // 6. Save
    await _dbContext.SaveChangesAsync();
}
```

#### Delete (Soft Delete)
```csharp
public async Task Delete(Guid roleId)
{
    // 1. Find entity
    var entity = await _dbContext.Set<RoleEntity>()
        .FirstOrDefaultAsync(x => x.RoleId == roleId && !x.IsDeleted);
    if (entity == null)
        throw new Exception("Role not found");
    
    // 2. Business rule validation (check if used)
    var usersCount = await _dbContext.Set<UserEntity>()
        .Where(u => u.RoleId == roleId && !u.IsDeleted)
        .CountAsync();
    if (usersCount > 0)
        throw new Exception($"Cannot delete. {usersCount} user(s) still using this role");
    
    // 3. Soft delete
    entity.IsDeleted = true;
    entity.UpdatedAt = DateTime.UtcNow;
    
    // 4. Save
    await _dbContext.SaveChangesAsync();
}
```

#### DataTable (Server-side processing)
```csharp
public async Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request)
{
    // 1. Base query (exclude deleted)
    var query = _dbContext.Set<RoleEntity>()
        .Where(x => !x.IsDeleted)
        .AsQueryable();
    
    var totalRecord = await query.CountAsync();
    
    // 2. Search filter
    if (!string.IsNullOrEmpty(request.Search))
    {
        var search = request.Search.ToLower();
        query = query.Where(x => 
            x.RoleName.ToLower().Contains(search) ||
            (x.Description != null && x.Description.ToLower().Contains(search))
        );
    }
    
    // 3. Additional filters
    if (request.IsActive.HasValue)
    {
        query = query.Where(x => x.IsActive == request.IsActive.Value);
    }
    
    // 4. Sorting
    query = request?.OrderBy?.ToLower() switch
    {
        "rolename" => request?.OrderType?.ToLower() == "asc" 
            ? query.OrderBy(x => x.RoleName) 
            : query.OrderByDescending(x => x.RoleName),
        "createdat" => request?.OrderType?.ToLower() == "asc" 
            ? query.OrderBy(x => x.CreatedAt) 
            : query.OrderByDescending(x => x.CreatedAt),
        _ => query.OrderByDescending(x => x.CreatedAt)
    };
    
    var totalFiltered = await query.CountAsync();
    
    // 5. Pagination & Projection
    var data = await query
        .Skip((request.Page - 1) * request.Size)
        .Take(request.Size)
        .Select(x => new RoleViewModel
        {
            RoleId = x.RoleId,
            RoleName = x.RoleName,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt
        })
        .ToListAsync();
    
    // 6. Return response
    return new BaseDatatableResponse
    {
        Draw = request.Draw,
        RecordsTotal = totalRecord,
        RecordsFiltered = totalFiltered,
        Data = data
    };
}
```

### 5. Controller Pattern
```csharp
[Authorize]
public class RoleController : BaseController
{
    private readonly IRoleService _roleService;
    
    public RoleController(IRoleService roleService) : base(roleService)
    {
        _roleService = roleService;
    }
    
    // GET: Main view
    public IActionResult Index()
    {
        ViewData["Title"] = "Role Management";
        return View();
    }
    
    // POST: DataTable
    [HttpPost]
    public async Task<IActionResult> Datatable([FromBody] DataTableRoleRequest request)
    {
        var result = await _roleService.Datatable(request);
        return Json(result);
    }
    
    // GET: Get by ID
    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _roleService.GetById(id);
        return Json(result);
    }
    
    // POST: Create
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
    
    // POST: Edit
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
    
    // DELETE: Delete
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
```

**Key Points:**
- Extend `BaseController`
- Add `[Authorize]` attribute
- Constructor inject service
- Use `_currentUserService.UserId` untuk audit
- Consistent response format: `{ success, message }`
- Try-catch untuk error handling

### 6. View Pattern (Index.cshtml)
```razor
@{
    ViewData["Title"] = "Role Management";
}

<div class="card">
    <div class="card-body">
        <!-- Filters -->
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
        
        <!-- DataTable -->
        <div class="table-responsive">
            <table id="role_datatable" class="table table-striped table-row-bordered gy-5 gs-7 border rounded w-100">
                <thead>
                    <tr>
                        <th>No</th>
                        <th>Role Name</th>
                        <th>Description</th>
                        <th>Status</th>
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
```

### 7. JavaScript Pattern (index.js)
```javascript
let dataTable;
let isEditMode = false;

$(document).ready(function () {
    initializeDataTable();
    initializeEventHandlers();
});

// Initialize DataTable
function initializeDataTable() {
    dataTable = $('#role_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Role/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    IsActive: $('#filter-status').val() === "" ? null : $('#filter-status').val() === "true"
                });
            }
        },
        columns: [
            { data: null, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'roleName' },
            { data: 'description' },
            { 
                data: 'isActive',
                render: (data) => data 
                    ? '<span class="badge badge-light-success">Active</span>'
                    : '<span class="badge badge-light-warning">Inactive</span>'
            },
            {
                data: 'roleId',
                orderable: false,
                render: (data) => `
                    <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}">
                        <i class='fa fa-pencil'></i>
                    </button>
                    <button class="btn btn-sm btn-icon btn-light-danger btn-delete" data-id="${data}">
                        <i class='fa fa-trash-alt'></i>
                    </button>
                `
            }
        ]
    });
}

// Event Handlers
function initializeEventHandlers() {
    // Add button
    var btnAdd = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        id: 'btn-add-role',
        html: '<i class="fa fa-add"></i> Add Role'
    });
    $('#toolbar-section-button').append(btnAdd);
    
    $(document).on('click', '#btn-add-role', () => openModal(false));
    $('#btn-apply-filter').on('click', () => dataTable.ajax.reload());
    $('#btn-save-role').on('click', saveRole);
    
    // Edit
    $('#role_datatable').on('click', '.btn-edit', function () {
        const roleId = $(this).data('id');
        openModal(true, roleId);
    });
    
    // Delete
    $('#role_datatable').on('click', '.btn-delete', function () {
        const roleId = $(this).data('id');
        deleteRole(roleId);
    });
}

// Open Modal
function openModal(editMode, roleId = null) {
    isEditMode = editMode;
    $('#form-role')[0].reset();
    $('#role-id').val('');
    
    if (editMode && roleId) {
        $('#modal-role-title').text('Edit Role');
        loadRoleData(roleId);
    } else {
        $('#modal-role-title').text('Add Role');
        $('#role-active').prop('checked', true);
    }
    
    $('#modal-role').modal('show');
}

// Load Role Data
function loadRoleData(roleId) {
    $.ajax({
        url: '/Role/GetById',
        type: 'GET',
        data: { id: roleId },
        success: function (data) {
            $('#role-id').val(data.roleId);
            $('#role-name').val(data.roleName);
            $('#role-description').val(data.description);
            $('#role-active').prop('checked', data.isActive);
        }
    });
}

// Save Role
function saveRole() {
    if (!$('#form-role')[0].checkValidity()) {
        $('#form-role')[0].reportValidity();
        return;
    }
    
    const roleId = $('#role-id').val();
    const url = roleId ? '/Role/Edit' : '/Role/Create';
    
    const data = {
        roleName: $('#role-name').val(),
        description: $('#role-description').val(),
        isActive: $('#role-active').is(':checked')
    };
    
    if (roleId) {
        data.roleId = roleId;
    }
    
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            Swal.fire('Success', response.message, 'success');
            $('#modal-role').modal('hide');
            dataTable.ajax.reload();
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
        }
    });
}

// Delete Role
function deleteRole(roleId) {
    Swal.fire({
        title: 'Delete selected data?',
        text: "This action cannot be undone.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Role/Delete',
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(roleId),
                success: function (response) {
                    Swal.fire('Deleted!', response.message, 'success');
                    dataTable.ajax.reload();
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
                }
            });
        }
    });
}
```

---

## � Auedit Trail Best Practices

### Why Store User Name Instead of User ID?

**Problem with storing User ID:**
- ❌ Need JOIN query every time to display audit info
- ❌ Performance overhead
- ❌ Complex queries
- ❌ If user deleted, ID becomes meaningless

**Solution: Store User Name directly:**
- ✅ No JOIN needed - direct display
- ✅ Better performance
- ✅ Simple queries
- ✅ Readable even if user deleted
- ✅ Audit trail remains intact

### Implementation Pattern

```csharp
// Get user name from UserId
var user = await _dbContext.Set<UserEntity>()
    .Where(u => u.UserId == userId)
    .Select(u => u.FullName ?? u.UserName)
    .FirstOrDefaultAsync();

// Store user name in audit fields
entity.CreatedBy = user ?? "System";
entity.UpdatedBy = user ?? "System";
```

### BaseEntity Audit Fields

```csharp
public abstract class BaseEntity
{
    // Audit fields - store USER NAME, not ID
    public string CreatedBy { get; set; } = string.Empty;  // User's FullName or UserName
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }                 // User's FullName or UserName
    public DateTime? UpdatedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }                 // User's FullName or UserName
}
```

### Display in UI

```csharp
// ViewModel - no need to join with User table
public class CustomerViewModel
{
    public string? CreatedBy { get; set; }      // Already contains user name
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }      // Already contains user name
    public DateTime? UpdatedAt { get; set; }
}

// Service - simple projection, no JOIN
var data = await query
    .Select(x => new CustomerViewModel
    {
        CustomerId = x.CustomerId,
        CustomerName = x.CustomerName,
        CreatedBy = x.CreatedBy,        // Direct access, no JOIN
        CreatedAt = x.CreatedAt,
        UpdatedBy = x.UpdatedBy,        // Direct access, no JOIN
        UpdatedAt = x.UpdatedAt
    })
    .ToListAsync();
```

### Helper Method (Optional)

Create a helper method in BaseService:

```csharp
public class BaseService : IBaseService
{
    protected readonly AppDbContext _dbContext;
    
    public BaseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // Helper method to get user name from user ID
    protected async Task<string> GetUserNameAsync(Guid userId)
    {
        var user = await _dbContext.Set<UserEntity>()
            .Where(u => u.UserId == userId)
            .Select(u => u.FullName ?? u.UserName)
            .FirstOrDefaultAsync();
        
        return user ?? "System";
    }
}

// Usage in service
public async Task Create(CreateCustomerRequest request, Guid userId)
{
    var userName = await GetUserNameAsync(userId);
    
    var entity = new CustomerEntity
    {
        // ... other properties
        CreatedBy = userName,
        CreatedAt = DateTime.UtcNow
    };
    
    await _dbContext.SaveChangesAsync();
}
```

### Migration Consideration

If you have existing data with User IDs in audit fields:

```sql
-- Migration script to convert User IDs to User Names
UPDATE Customers 
SET CreatedBy = (SELECT COALESCE(FullName, UserName) FROM Users WHERE CAST(UserId AS NVARCHAR(50)) = Customers.CreatedBy)
WHERE CreatedBy IS NOT NULL 
  AND CreatedBy != 'system'
  AND LEN(CreatedBy) = 36; -- GUID length

UPDATE Customers 
SET UpdatedBy = (SELECT COALESCE(FullName, UserName) FROM Users WHERE CAST(UserId AS NVARCHAR(50)) = Customers.UpdatedBy)
WHERE UpdatedBy IS NOT NULL 
  AND UpdatedBy != 'system'
  AND LEN(UpdatedBy) = 36; -- GUID length
```

---

## 📋 Checklist untuk Master Data Baru

Gunakan checklist ini saat membuat master data baru (Customers, Suppliers, Products, Tax):

### 1. Entity Layer
- [ ] Create `{Entity}Entity.cs` extends BaseEntity
- [ ] Add primary key: `{Entity}Id` (Guid)
- [ ] Add required fields (non-nullable)
- [ ] Add optional fields (nullable dengan `?`)
- [ ] Add `IsActive` field
- [ ] Create `{Entity}EntityConfiguration.cs`
- [ ] Configure table name, primary key, max lengths
- [ ] Add unique index untuk code/name
- [ ] Add index untuk IsActive
- [ ] Add DbSet ke AppDbContext
- [ ] Generate migration: `dotnet ef migrations add Add{Entity}Entity`
- [ ] Apply migration: `dotnet ef database update`

### 2. Models Layer
- [ ] Create `{Entity}ViewModel.cs` - untuk display
- [ ] Create `Create{Entity}Request.cs` - dengan validation attributes
- [ ] Create `Update{Entity}Request.cs` - extends Create + Id
- [ ] Create `DataTable{Entity}Request.cs` - extends BaseDatatableRequest

### 3. Service Layer
- [ ] Create `{Entity}Service.cs` dengan interface dan implementation
- [ ] Implement `Datatable()` method
- [ ] Implement `GetById()` method
- [ ] Implement `Create()` method dengan validation
- [ ] Implement `Edit()` method dengan validation
- [ ] Implement `Delete()` method (soft delete)
- [ ] Add additional methods as needed (ToggleStatus, GetActive, etc.)
- [ ] Register service di `AppServiceCollection.cs`

### 4. Controller Layer
- [ ] Create `{Entity}Controller.cs` extends BaseController
- [ ] Add `[Authorize]` attribute
- [ ] Implement `Index()` action
- [ ] Implement `Datatable()` endpoint [HttpPost]
- [ ] Implement `GetById()` endpoint [HttpGet]
- [ ] Implement `Create()` endpoint [HttpPost]
- [ ] Implement `Edit()` endpoint [HttpPost]
- [ ] Implement `Delete()` endpoint [HttpDelete]
- [ ] Add try-catch untuk semua endpoints

### 5. View Layer
- [ ] Create `Views/{Entity}/Index.cshtml`
- [ ] Add filter section
- [ ] Add DataTable HTML
- [ ] Add Create/Edit modal
- [ ] Add form fields dengan validation
- [ ] Add Scripts section

### 6. JavaScript Layer
- [ ] Create `wwwroot/custom/features/{entity}/index.js`
- [ ] Initialize DataTable dengan server-side processing
- [ ] Implement filter functionality
- [ ] Implement Create modal
- [ ] Implement Edit modal
- [ ] Implement Delete dengan confirmation
- [ ] Implement form validation
- [ ] Add toolbar button

### 7. Seeding & Menu
- [ ] Create `{Entity}Seed.cs` dengan sample data
- [ ] Add to `Seeder.cs`
- [ ] Add to `SeedController.cs`
- [ ] Add menu item di `MenuSeed.cs`

---

## 🎯 Quick Reference

### Naming Conventions
- **Entity**: `{Entity}Entity` (e.g., `RoleEntity`, `CustomerEntity`)
- **Table**: Plural (e.g., `Roles`, `Customers`)
- **Primary Key**: `{Entity}Id` (e.g., `RoleId`, `CustomerId`)
- **Service**: `I{Entity}Service` / `{Entity}Service`
- **Controller**: `{Entity}Controller`
- **View Folder**: `Views/{Entity}/`
- **JS Folder**: `wwwroot/custom/features/{entity}/`

### Common Fields
- **Required**: `{Entity}Code`, `{Entity}Name`
- **Optional**: `Description`, `Notes`, `ContactPerson`, `Phone`, `Email`
- **Status**: `IsActive` (bool)
- **Audit**: Inherited from BaseEntity

### Response Format
```json
{
    "success": true,
    "message": "Operation successful"
}
```

### Error Handling
```csharp
try
{
    // operation
    return Ok(new { success = true, message = "Success message" });
}
catch (Exception ex)
{
    return BadRequest(new { success = false, message = ex.Message });
}
```

---

## 📝 Notes

1. **Selalu gunakan soft delete** (`IsDeleted = true`) bukan hard delete
2. **Selalu cek business rules** sebelum delete (apakah masih digunakan?)
3. **Selalu set audit fields** (CreatedBy, UpdatedAt, dll)
4. **Store USER NAME in audit fields**, NOT User ID (for better performance and readability)
5. **Gunakan async/await** untuk semua database operations
6. **Filter `!x.IsDeleted`** di semua queries
7. **Validation di 2 tempat**: Client-side (JavaScript) dan Server-side (C#)
8. **Consistent naming**: Ikuti conventions yang sudah ada
9. **SweetAlert** untuk notifications
10. **Bootstrap modals** untuk forms
11. **DataTables** untuk listing dengan server-side processing
12. **Get user name before setting audit fields**: `var user = await GetUserNameAsync(userId);`

---

## 🔗 Reference Files

Untuk implementasi lengkap, lihat:
- `Services/RoleService.cs`
- `Controllers/RoleController.cs`
- `Views/Role/Index.cshtml`
- `wwwroot/custom/features/role/index.js`
- `Entities/Entity/RoleEntity.cs`
- `Entities/EntityConfigurations/RoleEntityConfiguration.cs`

---

**Last Updated**: 2024-11-28
**Version**: 1.0
**Status**: ✅ Production Ready
