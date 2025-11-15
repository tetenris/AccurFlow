# Implementation Plan

- [ ] 1. Create RoleEntity and configure database
  - Create RoleEntity.cs with all required properties (RoleId, RoleName, Description, IsActive, IsDeleted, audit fields)
  - Add RoleEntity DbSet to AppDbContext
  - Configure entity in OnModelCreating (indexes, constraints, default values)
  - _Requirements: 6.1, 6.2_

- [ ] 2. Create data models and DTOs
  - [ ] 2.1 Create RoleViewModel.cs
    - Define RoleViewModel class with all display properties
    - _Requirements: 1.1, 5.1_
  
  - [ ] 2.2 Create request models
    - Create CreateRoleRequest.cs for creating new roles
    - Create EditRoleRequest.cs for updating roles
    - Create DataTableRoleRequest.cs extending BaseDatatableRequest
    - _Requirements: 2.1, 3.1, 1.5_

- [ ] 3. Implement RoleService
  - [ ] 3.1 Create IRoleService interface
    - Define interface extending IBaseService
    - Declare methods: Datatable, GetById, Create, Edit, Delete
    - _Requirements: 6.2_
  
  - [ ] 3.2 Implement Datatable method
    - Implement server-side datatable processing
    - Add search functionality for RoleName and Description
    - Implement sorting by RoleName, IsActive, CreatedAt
    - Add IsActive filter support
    - Implement pagination logic
    - _Requirements: 1.1, 1.2, 1.3, 1.5_
  
  - [ ] 3.3 Implement GetById method
    - Query role by RoleId
    - Filter out deleted roles
    - Map to RoleViewModel
    - _Requirements: 5.1, 5.2_
  
  - [ ] 3.4 Implement Create method
    - Validate input data
    - Create new RoleEntity with generated Guid
    - Set CreatedBy and CreatedAt from current user
    - Save to database
    - _Requirements: 2.2, 2.3, 2.5, 6.5_
  
  - [ ] 3.5 Implement Edit method
    - Find existing role by RoleId
    - Validate role exists and not deleted
    - Update role properties
    - Set UpdatedBy and UpdatedAt from current user
    - Save changes to database
    - _Requirements: 3.2, 3.3, 3.5_
  
  - [ ] 3.6 Implement Delete method
    - Find existing role by RoleId
    - Validate role exists and not deleted
    - Set IsDeleted flag to true
    - Set UpdatedAt timestamp
    - Save changes to database
    - _Requirements: 4.2, 4.3_

- [ ] 4. Implement RoleController
  - [ ] 4.1 Create RoleController extending BaseController
    - Inject IRoleService dependency
    - Set up constructor
    - _Requirements: 6.1_
  
  - [ ] 4.2 Implement Index action
    - Return Index view
    - Set ViewData Title
    - _Requirements: 1.1_
  
  - [ ] 4.3 Implement Datatable action
    - Create HttpPost action accepting DataTableRoleRequest
    - Call service Datatable method
    - Return JSON result
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [ ] 4.4 Implement GetById action
    - Create HttpGet action accepting Guid id
    - Call service GetById method
    - Return JSON result
    - _Requirements: 3.1, 5.1_
  
  - [ ] 4.5 Implement Create action
    - Create HttpPost action accepting CreateRoleRequest
    - Get current user from ICurrentUserService
    - Call service Create method
    - Return success/error JSON response with try-catch
    - _Requirements: 2.1, 2.5_
  
  - [ ] 4.6 Implement Edit action
    - Create HttpPost action accepting EditRoleRequest
    - Get current user from ICurrentUserService
    - Call service Edit method
    - Return success/error JSON response with try-catch
    - _Requirements: 3.1, 3.4_
  
  - [ ] 4.7 Implement Delete action
    - Create HttpDelete action accepting Guid id
    - Call service Delete method
    - Return success/error JSON response with try-catch
    - _Requirements: 4.1, 4.5_

- [ ] 5. Create Index view
  - [ ] 5.1 Create Index.cshtml structure
    - Create card layout with header and body
    - Add "Add Role" button in card header
    - Create filter section with status dropdown
    - Add table element with id "role_datatable"
    - _Requirements: 1.1, 2.1_
  
  - [ ] 5.2 Create modal form for Add/Edit
    - Create Bootstrap modal with id "modal-role"
    - Add form fields: RoleName (required), Description (textarea), IsActive (checkbox)
    - Add hidden field for RoleId
    - Add Save and Cancel buttons
    - _Requirements: 2.1, 3.1_
  
  - [ ] 5.3 Add table columns
    - Define columns: No, Role Name, Description, Status, Created At, Action
    - Add action buttons: Edit and Delete
    - _Requirements: 1.1, 1.4_

- [ ] 6. Implement JavaScript functionality
  - [ ] 6.1 Initialize DataTable
    - Configure DataTable with server-side processing
    - Set up AJAX call to /Role/Datatable endpoint
    - Configure columns mapping
    - Implement custom rendering for Status column (badge)
    - Implement custom rendering for Created At (moment.js formatting)
    - Implement custom rendering for Action buttons
    - _Requirements: 1.1, 1.2, 1.3, 1.4_
  
  - [ ] 6.2 Implement filter functionality
    - Add event handler for Apply Filter button
    - Reload DataTable with filter parameters
    - _Requirements: 1.5_
  
  - [ ] 6.3 Implement Add Role functionality
    - Add click handler for Add Role button
    - Reset form and set modal title to "Add Role"
    - Show modal
    - _Requirements: 2.1_
  
  - [ ] 6.4 Implement Edit Role functionality
    - Add click handler for Edit buttons
    - Load role data via AJAX call to GetById
    - Populate form fields with role data
    - Set modal title to "Edit Role"
    - Show modal
    - _Requirements: 3.1, 3.2_
  
  - [ ] 6.5 Implement Save functionality
    - Add click handler for Save button
    - Validate required fields
    - Determine if Add or Edit mode
    - Send AJAX request to Create or Edit endpoint
    - Show success message and reload table on success
    - Show error message on failure
    - _Requirements: 2.5, 3.4_
  
  - [ ] 6.6 Implement Delete functionality
    - Add click handler for Delete buttons
    - Show SweetAlert confirmation dialog
    - Send AJAX DELETE request on confirmation
    - Show success message and reload table on success
    - Show error message on failure
    - _Requirements: 4.1, 4.5_

- [ ] 7. Register services and run migrations
  - [ ] 7.1 Register IRoleService in dependency injection
    - Add service registration in Program.cs or AppServiceCollection.cs
    - Register as scoped service
    - _Requirements: 6.1, 6.2_
  
  - [ ] 7.2 Create and run database migration
    - Run: dotnet ef migrations add AddRoleEntity
    - Review generated migration
    - Run: dotnet ef database update
    - Verify table created in database
    - _Requirements: 6.1_

- [ ]* 8. Testing and validation
  - [ ]* 8.1 Test Create functionality
    - Test creating role with all fields
    - Test creating role with only required fields
    - Test validation for required fields
    - Verify audit fields are populated correctly
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_
  
  - [ ]* 8.2 Test Read/List functionality
    - Test datatable displays all roles
    - Test pagination works correctly
    - Test sorting by different columns
    - Test search functionality
    - Test status filter
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_
  
  - [ ]* 8.3 Test Update functionality
    - Test editing role name
    - Test editing description
    - Test toggling active status
    - Verify UpdatedBy and UpdatedAt are set
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_
  
  - [ ]* 8.4 Test Delete functionality
    - Test soft delete sets IsDeleted flag
    - Verify role is removed from list
    - Verify role still exists in database
    - Test delete confirmation dialog
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_
