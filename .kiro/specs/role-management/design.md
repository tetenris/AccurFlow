# Design Document

## Overview

Role Management module akan mengimplementasikan CRUD operations untuk mengelola roles dalam sistem AccuFlow. Module ini akan menggunakan pattern yang sudah ada di project (BaseController, BaseService) dan mengikuti struktur yang sama dengan module lain untuk konsistensi.

Fitur ini akan menyediakan:
- DataTable dengan server-side processing untuk performa optimal
- Modal-based form untuk Add/Edit operations
- Soft delete untuk data integrity
- Audit trail (CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
- Real-time validation dan user feedback

## Architecture

### Component Structure

```
AccuFlow/
├── Entities/
│   └── RoleEntity.cs
├── Models/
│   └── Role/
│       ├── RoleViewModel.cs
│       ├── CreateRoleRequest.cs
│       ├── EditRoleRequest.cs
│       └── DataTableRoleRequest.cs
├── Services/
│   ├── IRoleService.cs
│   └── RoleService.cs
├── Controllers/
│   └── RoleController.cs
├── Views/
│   └── Role/
│       └── Index.cshtml
└── wwwroot/
    └── custom/
        └── features/
            └── role/
                └── index.js
```

### Design Pattern

1. **Repository Pattern**: Service layer bertindak sebagai repository untuk data access
2. **DTO Pattern**: Menggunakan ViewModel dan Request models untuk data transfer
3. **Soft Delete Pattern**: IsDeleted flag untuk menandai deleted records
4. **Audit Pattern**: Tracking CreatedBy, CreatedAt, UpdatedBy, UpdatedAt

## Components and Interfaces

### 1. RoleEntity.cs

```csharp
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
```

### 2. IRoleService Interface

```csharp
public interface IRoleService : IBaseService
{
    Task<BaseDatatableResponse> Datatable(DataTableRoleRequest request);
    Task<RoleViewModel?> GetById(Guid roleId);
    Task Create(CreateRoleRequest request, Guid userId);
    Task Edit(EditRoleRequest request, Guid userId);
    Task Delete(Guid roleId);
}
```

### 3. RoleService Implementation

**Key Methods:**

- **Datatable**: Server-side processing dengan filtering, sorting, dan pagination
- **GetById**: Retrieve single role by ID
- **Create**: Insert new role dengan audit info
- **Edit**: Update existing role dengan audit info
- **Delete**: Soft delete dengan IsDeleted flag

**Datatable Logic:**
```csharp
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
```

### 4. RoleController

**Endpoints:**
- `GET /Role/Index` - Display role management page
- `POST /Role/Datatable` - Get datatable data
- `GET /Role/GetById?id={guid}` - Get single role
- `POST /Role/Create` - Create new role
- `POST /Role/Edit` - Update existing role
- `DELETE /Role/Delete` - Soft delete role

**Controller Structure:**
```csharp
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

    // ... other actions
}
```

## Data Models

### RoleViewModel
```csharp
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
```

### CreateRoleRequest
```csharp
public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### EditRoleRequest
```csharp
public class EditRoleRequest
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
```

### DataTableRoleRequest
```csharp
public class DataTableRoleRequest : BaseDatatableRequest
{
    public bool? IsActive { get; set; }
}
```

## Error Handling

### Validation
- Role name is required (client-side and server-side)
- Description max length 500 characters
- Duplicate role name check (optional enhancement)

### Exception Handling
```csharp
try
{
    await _roleService.Create(request, _currentUserService.UserId);
    return Ok(new { success = true, message = "Role created successfully" });
}
catch (Exception ex)
{
    return BadRequest(new { success = false, message = ex.Message });
}
```

### User Feedback
- Success messages using SweetAlert2
- Error messages with specific error details
- Confirmation dialogs for delete operations
- Loading indicators during async operations

## Testing Strategy

### Manual Testing Checklist
1. **Create Role**
   - Create role with all fields
   - Create role with only required fields
   - Validate required field validation
   - Verify audit fields are populated

2. **Read/List Roles**
   - View all roles in datatable
   - Test pagination
   - Test sorting by different columns
   - Test search functionality
   - Test status filter

3. **Update Role**
   - Edit role name
   - Edit description
   - Toggle active status
   - Verify UpdatedBy and UpdatedAt are set

4. **Delete Role**
   - Delete role and verify soft delete
   - Verify role is removed from list
   - Verify role still exists in database with IsDeleted=true

### Integration Testing
- Test with empty database
- Test with existing roles
- Test concurrent operations
- Test with large dataset (performance)

## UI Design

### Index Page Layout

```
+--------------------------------------------------+
|  Role Management                    [+ Add Role] |
+--------------------------------------------------+
|  Status: [All Status ▼]  [Apply Filter]         |
+--------------------------------------------------+
|  No | Role Name | Description | Status | Created |
|-----|-----------|-------------|--------|---------|
|  1  | Admin     | Full access | Active | 01-Jan  |
|  2  | User      | Basic       | Active | 02-Jan  |
+--------------------------------------------------+
|  Showing 1 to 10 of 50 entries      [< 1 2 3 >] |
+--------------------------------------------------+
```

### Modal Form

```
+--------------------------------+
|  Add Role               [X]    |
+--------------------------------+
|  Role Name: *                  |
|  [________________]            |
|                                |
|  Description:                  |
|  [________________]            |
|  [________________]            |
|  [________________]            |
|                                |
|  [✓] Active                    |
|                                |
|  [Cancel]  [Save]              |
+--------------------------------+
```

### Features
- Responsive design using Bootstrap 5
- DataTables for table functionality
- SweetAlert2 for notifications
- jQuery for AJAX operations
- Moment.js for date formatting

## Database Schema

### RoleEntity Table

| Column | Type | Constraints |
|--------|------|-------------|
| RoleId | UNIQUEIDENTIFIER | PRIMARY KEY |
| RoleName | NVARCHAR(100) | NOT NULL, INDEX |
| Description | NVARCHAR(500) | NULL |
| IsActive | BIT | NOT NULL, DEFAULT 1 |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 |
| CreatedBy | UNIQUEIDENTIFIER | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedBy | UNIQUEIDENTIFIER | NULL |
| UpdatedAt | DATETIME2 | NULL |

### Indexes
- Primary Key on RoleId
- Non-clustered index on RoleName for search performance
- Consider index on IsDeleted for filtering

## Security Considerations

### Authorization
- Only authenticated users can access role management
- Consider role-based authorization (only Admin can manage roles)
- Add `[Authorize]` attribute to controller

### Input Validation
- Sanitize user input to prevent XSS
- Validate max lengths
- Validate required fields
- Use parameterized queries (EF Core handles this)

### Audit Trail
- Track who created the role (CreatedBy)
- Track when role was created (CreatedAt)
- Track who last updated (UpdatedBy)
- Track when last updated (UpdatedAt)

## Dependencies

### NuGet Packages
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- System.Linq.Dynamic.Core (for dynamic sorting)

### Frontend Libraries
- jQuery 3.x
- DataTables 1.13.x
- Bootstrap 5.x
- SweetAlert2
- Moment.js

### Existing Infrastructure
- BaseController
- BaseService
- IBaseService
- BaseDatatableRequest
- BaseDatatableResponse
- ICurrentUserService
- AppDbContext
