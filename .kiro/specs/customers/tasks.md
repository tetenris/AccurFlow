# Implementation Plan - Customer Management

## Overview

This implementation plan breaks down the Customer Management feature into discrete, manageable coding tasks. Each task builds incrementally on previous tasks, following a bottom-up approach: data layer → business logic → API → UI.

---

## Tasks

- [x] 1. Create CustomerEntity and database migration


  - Create `CustomerEntity` class with all required properties (CustomerId, CustomerCode, CustomerName, CustomerType, contact info, address, financial info, tax info, notes, IsActive)
  - Create entity configuration class `CustomerEntityConfiguration` with proper constraints, indexes, and default values
  - Generate and apply EF Core migration to create Customers table
  - _Requirements: 1.1, 1.2, 1.3, 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7, 11.8, 11.9_


- [ ] 2. Create view models and request/response DTOs
  - [x] 2.1 Create `CustomerViewModel` for displaying customer data

    - Include all customer properties
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8_
  

  - [ ] 2.2 Create `CustomerDropdownViewModel` for dropdown lists
    - Include CustomerId, CustomerCode, CustomerName, DisplayText
    - _Requirements: 10.1, 10.2_
  
  - [x] 2.3 Create `CreateCustomerRequest` with validation attributes

    - Add data annotations for required fields, string lengths, email format, range validation
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 11.1-11.10_
  

  - [ ] 2.4 Create `UpdateCustomerRequest` with validation attributes
    - Extend CreateCustomerRequest and add CustomerId
    - _Requirements: 4.1, 4.2, 4.3, 4.5, 4.6_

  
  - [x] 2.5 Create `DataTableCustomerRequest` for filtering and pagination

    - Extend `BaseDatatableRequest` with CustomerType and IsActive filters
    - _Requirements: 2.3, 2.4, 2.5_

- [x] 3. Implement CustomerService with core business logic


  - [x] 3.1 Create `ICustomerService` interface

    - Define all service methods (CRUD, validation, status, utility, export)
    - _Requirements: All_
  

  - [ ] 3.2 Implement `CustomerService` class with CRUD operations
    - Implement `CreateAsync` method with validation (code uniqueness, required fields)
    - Implement `UpdateAsync` method with validation (existence, code uniqueness)
    - Implement `DeleteAsync` method with business rules (check transactions, soft delete)
    - Implement `GetByIdAsync` and `GetByCodeAsync` methods
    - _Requirements: 1.1-1.10, 4.1-4.6, 5.1-5.6_

  
  - [ ] 3.3 Implement validation methods
    - Implement `IsCodeUniqueAsync` to check customer code uniqueness
    - Implement `CanDeleteAsync` to validate deletion rules
    - Implement `HasTransactionsAsync` to check if customer is used in transactions

    - _Requirements: 5.3, 5.4, 7.1, 7.2, 7.3_
  
  - [ ] 3.4 Implement status management methods
    - Implement `ToggleStatusAsync` to activate/deactivate customers

    - Implement `GetActiveCustomersAsync` to get only active customers for dropdowns
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 10.1, 10.2, 10.3, 10.4_
  
  - [ ] 3.5 Implement datatable method with filtering and pagination
    - Implement `Datatable` method with support for search, filters (type, status), sorting, and pagination


    - Search across CustomerCode, CustomerName, ContactPerson, Phone, Email
    - Use dynamic LINQ for flexible sorting
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7_
  
  - [ ] 3.6 Implement utility methods
    - Implement `GenerateCustomerCodeAsync` to generate next sequential code (CUST-00001 format)
    - Implement `ExportToExcelAsync` to export customer list to Excel with NPOI
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3, 9.4, 9.5_

- [x] 4. Register CustomerService in dependency injection

  - Add service registration in `AppServiceCollection.cs`
  - _Requirements: All_


- [ ] 5. Create CustomerController with API endpoints
  - [ ] 5.1 Create `CustomerController` class extending `BaseController`
    - Inject `ICustomerService` dependency
    - _Requirements: All_
  
  - [ ] 5.2 Implement Index action for main view
    - Return view with ViewData title
    - _Requirements: 2.1, 12.1, 12.2_
  
  - [ ] 5.3 Implement Datatable endpoint for listing customers
    - POST endpoint accepting `DataTableCustomerRequest`
    - Return JSON with paginated and filtered results
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7_
  
  - [ ] 5.4 Implement GetById endpoint
    - GET endpoint accepting customer ID
    - Return customer details as JSON
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8_
  
  - [ ] 5.5 Implement GetActiveCustomers endpoint
    - GET endpoint returning list of active customers for dropdown
    - _Requirements: 10.1, 10.2, 10.3, 10.4_
  
  - [ ] 5.6 Implement Create endpoint
    - POST endpoint accepting `CreateCustomerRequest`
    - Return success/error response with appropriate HTTP status codes
    - Include try-catch for error handling
    - _Requirements: 1.1-1.10, 12.3_
  
  - [ ] 5.7 Implement Edit endpoint
    - POST endpoint accepting `UpdateCustomerRequest`
    - Return success/error response
    - _Requirements: 4.1-4.6, 12.4_
  
  - [ ] 5.8 Implement Delete endpoint
    - DELETE endpoint accepting customer ID
    - Return success/error response with validation messages
    - _Requirements: 5.1-5.6, 12.5_
  
  - [ ] 5.9 Implement ToggleStatus endpoint
    - POST endpoint accepting customer ID
    - Toggle between active and inactive status
    - _Requirements: 6.1-6.5_
  
  - [ ] 5.10 Implement ValidateCode endpoint
    - GET endpoint for real-time code validation
    - Accept code and optional excludeId parameters
    - Return boolean indicating if code is available
    - _Requirements: 7.1, 7.2, 7.3, 7.4_
  
  - [ ] 5.11 Implement GenerateCode endpoint
    - GET endpoint to generate next customer code
    - Return generated code
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  
  - [ ] 5.12 Implement ExportExcel endpoint
    - GET endpoint with filter parameters
    - Return Excel file
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_

- [ ] 6. Create Index view (Views/Customer/Index.cshtml)
  - [ ] 6.1 Create view file with page layout
    - Add page title and "Add New Customer" button
    - Add filter section (Customer Type dropdown, Status dropdown, Search input)
    - Add Export button
    - _Requirements: 2.1, 2.3, 2.4, 2.5_
  
  - [ ] 6.2 Add DataTable HTML structure
    - Create table with columns: Code, Name, Type, Contact Person, Phone, Email, Status, Actions
    - Add action buttons (View, Edit, Delete, Toggle Status) for each row
    - _Requirements: 2.2_
  
  - [ ] 6.3 Add Create/Edit modal HTML
    - Create modal dialog with form fields organized in tabs or sections:
      - Basic Info: Customer Code, Name, Type
      - Contact Info: Contact Person, Phone, Email, Website
      - Address: Address, City, State, Postal Code, Country
      - Financial: Credit Limit, Payment Terms
      - Tax: Tax ID
      - Additional: Notes, IsActive checkbox
    - Add validation message placeholders
    - Add Cancel and Save buttons
    - _Requirements: 1.1-1.10, 4.1-4.6_
  
  - [ ] 6.4 Add Detail modal HTML
    - Create read-only modal showing all customer information
    - Display in organized sections matching the edit form
    - Show audit information (Created By, Created At, Updated By, Updated At)
    - _Requirements: 3.1-3.8_

- [ ] 7. Create JavaScript file for Index page (wwwroot/custom/features/customer/index.js)
  - [ ] 7.1 Initialize DataTable with server-side processing
    - Configure DataTable with AJAX source pointing to Datatable endpoint
    - Define columns with custom rendering for customer type badges, status badges, and action buttons
    - _Requirements: 2.1, 2.2, 2.6, 2.7_
  
  - [ ] 7.2 Implement filter functionality
    - Add event handlers for Customer Type, Status, and Search filters
    - Reload DataTable when filters change
    - _Requirements: 2.3, 2.4, 2.5_
  
  - [ ] 7.3 Implement Create modal functionality
    - Show modal on "Add New Customer" button click
    - Implement real-time customer code validation
    - Auto-generate customer code on modal open
    - Handle form submission with AJAX POST to Create endpoint
    - Show success/error messages using SweetAlert
    - Reload DataTable on successful creation
    - _Requirements: 1.1-1.10, 7.1-7.4, 8.1-8.4_
  
  - [ ] 7.4 Implement Edit modal functionality
    - Load customer data on Edit button click
    - Pre-fill form fields with existing data
    - Handle form submission with AJAX POST to Edit endpoint
    - Reload DataTable on successful update
    - _Requirements: 4.1-4.6_
  
  - [ ] 7.5 Implement Delete functionality
    - Show confirmation dialog on Delete button click
    - Send AJAX DELETE request to Delete endpoint
    - Handle validation errors (customer has transactions)
    - Reload DataTable on successful deletion
    - _Requirements: 5.1-5.6_
  
  - [ ] 7.6 Implement Toggle Status functionality
    - Send AJAX POST request to ToggleStatus endpoint on status button click
    - Update row status badge without full page reload
    - _Requirements: 6.1-6.5_
  
  - [ ] 7.7 Implement Detail modal functionality
    - Load customer data on View button click
    - Display all customer information in read-only format
    - Show audit information
    - _Requirements: 3.1-3.8_
  
  - [ ] 7.8 Implement form validation
    - Add client-side validation for required fields
    - Add format validation for email and phone
    - Add range validation for credit limit and payment terms
    - Display validation error messages
    - _Requirements: 11.1-11.10_
  
  - [ ] 7.9 Implement customer code auto-generation
    - Call GenerateCode endpoint when modal opens for new customer
    - Pre-fill customer code input with generated code
    - Allow user to override generated code
    - _Requirements: 8.1-8.4_
  
  - [ ] 7.10 Implement Export functionality
    - Trigger export with active filters
    - Download Excel file
    - _Requirements: 9.1-9.5_

- [ ] 8. Create seed data for Customers
  - [ ] 8.1 Create `CustomerSeed.cs` with sample customers
    - Define seed data for different customer types (Individual, Corporate, Government)
    - Include various contact and address information
    - Set different credit limits and payment terms
    - _Requirements: All_
  
  - [ ] 8.2 Add SeedCustomers method to `Seeder.cs`
    - Implement seeding logic with duplicate check
    - _Requirements: All_
  
  - [ ] 8.3 Update SeedController to include Customers seeder
    - Add Customers option to seed page
    - _Requirements: All_


- [ ] 9. Add Customers menu to MenuSeed
  - Update `MenuSeed.cs` to add "Customers" menu item under Master menu (already exists)
  - _Requirements: All_

- [ ]* 10. Test the complete Customer Management feature
  - [ ]* 10.1 Test customer creation with various scenarios
    - Create customers with all customer types
    - Test validation errors (duplicate code, invalid email, invalid phone)
    - Test required field validation
    - _Requirements: 1.1-1.10, 11.1-11.10_
  
  - [ ]* 10.2 Test customer editing
    - Edit customer information
    - Change customer type
    - Update contact and address information
    - Test validation errors
    - _Requirements: 4.1-4.6_
  
  - [ ]* 10.3 Test customer deletion
    - Delete customer without transactions
    - Attempt to delete customer with transactions (should fail)
    - Verify soft delete (data retained)
    - _Requirements: 5.1-5.6_
  
  - [ ]* 10.4 Test status toggle
    - Deactivate customer and verify it doesn't appear in dropdowns
    - Reactivate customer
    - _Requirements: 6.1-6.5, 10.1-10.4_
  
  - [ ]* 10.5 Test search and filters
    - Search by customer code, name, contact, phone, email
    - Filter by customer type
    - Filter by status
    - _Requirements: 2.1-2.7_
  
  - [ ]* 10.6 Test code generation and validation
    - Generate customer code
    - Test code uniqueness validation
    - Test duplicate code prevention
    - _Requirements: 7.1-7.4, 8.1-8.4_
  
  - [ ]* 10.7 Test export functionality
    - Export with various filters
    - Verify Excel format and content
    - _Requirements: 9.1-9.5_

---

## Notes

- Each task should be completed and tested before moving to the next
- Follow the established coding standards and naming conventions from Chart of Accounts module
- Ensure all validation rules are implemented both client-side and server-side
- Test edge cases and error scenarios thoroughly
- Use NPOI library for Excel export (already installed)
- Customer Type values: "Individual", "Corporate", "Government"
