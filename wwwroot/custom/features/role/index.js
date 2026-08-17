$(document).ready(function () {
    let isEditMode = false;
    let isPermissionMode = false;
    let currentRoleId = null;

    const roleTypeData = window.roleTypeData;

    // Load Role Dropdown
    function loadRoleDropdown() {
        return $.get('/Role/GetRoleDropdown', function (data) {
            const $select = $('#role-type');
            $select.empty();
            $select.append('<option value="">Select Role Type</option>');
            $.each(data, function (index, item) {
                $select.append($('<option>', {
                    value: item.value,
                    text: item.text
                }));
            });
        });
    }

    // Load dropdown on page load
    loadRoleDropdown();

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
            searchPlaceholder: 'Search'
        },
        initComplete: function (settings, json) {
            if ($('#role_datatable_length').find('span.mr-2').length === 0) {
                $('#role_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            // Add search icon to filter
            $('#role_datatable_filter').css('position', 'relative');
            $('#role_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');

            // Style search box to match status dropdown
            $('#role_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            // Style show dropdown to match status dropdown
            $('#role_datatable_length select').css({
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 2rem 0.5rem 0.75rem',
                'height': '42px',
                'font-size': '1rem'
            });
            // Make top section flex
            $('.top').css({
                'display': 'flex',
                'align-items': 'center',
                'gap': '15px'
            });
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
        order: [[1, 'asc']],
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
                        : '<span class="badge badge-light-warning">Inactive</span>';
                }
            },
            {
                data: "roleId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    const isSuperAdmin = row.roleType === 1;
                    const editButton = isSuperAdmin ? '' : `
                            <button class="btn btn-sm btn-icon btn-light-warning btn-edit" data-id="${data}" title="Edit Role">
                                <i class='fa fa-pen'></i>
                            </button>`;
                    const permissionButton = isSuperAdmin ? '' : `
                            <button class="btn btn-sm btn-icon btn-light-info btn-permission" data-id="${data}" title="Manage Permissions">
                                <i class='fa fa-key'></i>
                            </button>`;

                    return `
                        <div class="d-flex gap-2 justify-content-center">
                            ${editButton}
                            <button class="btn btn-sm btn-icon btn-light-primary btn-detail" data-id="${data}" title="View Detail">
                                <i class='fa fa-eye'></i>
                            </button>
                            ${permissionButton}
                            <button class="btn btn-sm btn-icon btn-light btn-audit" 
                                data-id="${data}" 
                                data-createdby="${row.createdBy}" 
                                data-createdat="${row.createdAt}" 
                                data-updatedby="${row.updatedBy}" 
                                data-updatedat="${row.updatedAt}" 
                                title="Audit Trail">
                                <i class='fa fa-file-alt'></i>
                            </button>
                        </div>
                    `;
                }
            }
        ]
    });

    // Apply Filter
    $('#btn-apply-filter').on('click', function () {
        table.ajax.reload();
    });

    // Role Type mapping
    // Role Type Change
    $('#role-type').on('change', function () {
        const selectedType = $(this).val();

        if (selectedType && roleTypeData[selectedType]) {
            const roleData = roleTypeData[selectedType];
            $('#role-name').val(roleData.name).prop('readonly', true);
            $('#role-description').val(roleData.description).prop('readonly', true);
        } else {
            $('#role-name').val('').prop('readonly', false);
            $('#role-description').val('').prop('readonly', false);
        }
    });

    // Add Role (using event delegation for dynamically added button)
    $(document).on('click', '#btn-add-role', function () {
        isEditMode = false;
        isPermissionMode = false;
        currentRoleId = null;
        $('#modal-role-title').text('Add Role');
        $('#form-role')[0].reset();

        // Enable all fields for adding
        $('#role-type').val('').prop('disabled', false).removeClass('bg-light');
        $('#role-name').val('').prop('readonly', false);
        $('#role-description').val('').prop('readonly', false);
        $('#role-active').prop('checked', true).prop('disabled', false);
        $('#btn-save-role').text('Save').removeClass('d-none');
        
        // Hide hint for add mode
        $('#role-type-hint').addClass('d-none');

        // Load permissions for new role (empty roleId)
        loadMenuPermissions('');

        $('#modal-role').modal('show');
    });

    // Add button to toolbar
    var btnAdd = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-add-role',
        html: '<i class="fa fa-add"></i> Add New Role'
    });
    $('#toolbar-section-button').append(btnAdd);

    // Edit Role (navigate to edit page)
    $('#role_datatable').on('click', '.btn-edit', function () {
        const roleId = $(this).data('id');
        window.location.href = `/Role/Edit?id=${roleId}`;
    });

    // Manage Role Permissions (navigate to edit page)
    $('#role_datatable').on('click', '.btn-permission', function () {
        const roleId = $(this).data('id');
        window.location.href = `/Role/Edit?id=${roleId}`;
    });

    // Save Role (Add mode only; Edit uses dedicated page)
    $('#btn-save-role').on('click', async function () {
        // Temporarily enable disabled fields to get their values
        $('#role-type').prop('disabled', false);

        const roleType = $('#role-type').val();
        const roleName = $('#role-name').val().trim();

        if (!roleType) {
            Swal.fire('Warning', 'Role type is required', 'warning');
            return;
        }

        if (!roleName) {
            Swal.fire('Warning', 'Role name is required', 'warning');
            return;
        }

        const data = {
            RoleType: parseInt(roleType),
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
                // Get the roleId (from response for new role, or currentRoleId for edit)
                const savedRoleId = response.roleId || currentRoleId;
                
                // Save permissions
                const permissions = collectPermissions();
                console.log('Collected permissions:', permissions);
                console.log('Total permissions:', permissions.length);
                
                await $.ajax({
                    url: '/Role/SaveRoleMenuPermissions',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        roleId: savedRoleId,
                        permissions: permissions
                    })
                });

                Swal.fire({
                    title: 'Success',
                    text: 'Role and permissions saved successfully',
                    icon: 'success',
                    confirmButtonText: 'OK',
                    customClass: {
                        confirmButton: 'btn btn-success'
                    },
                    buttonsStyling: false
                });
                $('#modal-role').modal('hide');
                table.ajax.reload();
            }
        } catch (error) {
            Swal.fire({
                title: 'Error',
                text: error.responseJSON?.message || 'Failed to save role',
                icon: 'error',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
        }
    });

    // Detail Role
    $('#role_datatable').on('click', '.btn-detail', async function () {
        const roleId = $(this).data('id');

        try {
            const response = await $.get(`/Role/GetById?id=${roleId}`);

            const roleTypeText = roleTypeData[String(response.roleType)]?.name || 'Unknown';
            const statusText = response.isActive ? 'Active' : 'Inactive';

            Swal.fire({
                title: 'Role Details',
                html: `
                    <div class="text-start">
                        <table class="table table-row-bordered">
                            <tr>
                                <td class="fw-bold" width="40%">Role Type</td>
                                <td>${roleTypeText}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Role Name</td>
                                <td>${response.roleName}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Description</td>
                                <td>${response.description || '-'}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Status</td>
                                <td>${statusText}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Created At</td>
                                <td>${moment(response.createdAt).format('DD-MMM-YYYY HH:mm')}</td>
                            </tr>
                            ${response.updatedAt ? `
                            <tr>
                                <td class="fw-bold">Updated At</td>
                                <td>${moment(response.updatedAt).format('DD-MMM-YYYY HH:mm')}</td>
                            </tr>` : ''}
                        </table>
                    </div>
                `,
                confirmButtonText: 'Close',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false,
                width: '600px'
            });
        } catch (error) {
            Swal.fire({
                title: 'Error',
                text: 'Failed to load role details',
                icon: 'error',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
        }
    });

    // Audit Trail
    $('#role_datatable').on('click', '.btn-audit', function () {
        const createdBy = $(this).data('createdby');
        const createdAt = $(this).data('createdat');
        const updatedBy = $(this).data('updatedby');
        const updatedAt = $(this).data('updatedat');

        Swal.fire({
            title: 'Audit Trail',
            html: `
                <div class="text-start">
                    <table class="table table-row-bordered">
                        <tr>
                            <td class="fw-bold" width="40%">Created By</td>
                            <td>${createdBy || '-'}</td>
                        </tr>
                        <tr>
                            <td class="fw-bold">Created At</td>
                            <td>${createdAt ? moment(createdAt).format('DD-MMM-YYYY HH:mm:ss') : '-'}</td>
                        </tr>
                        <tr>
                            <td class="fw-bold">Updated By</td>
                            <td>${updatedBy || '-'}</td>
                        </tr>
                        <tr>
                            <td class="fw-bold">Updated At</td>
                            <td>${updatedAt ? moment(updatedAt).format('DD-MMM-YYYY HH:mm:ss') : '-'}</td>
                        </tr>
                    </table>
                </div>
            `,
            confirmButtonText: 'Close',
            customClass: {
                confirmButton: 'btn btn-primary'
            },
            buttonsStyling: false,
            width: '500px'
        });
    });

    // Delete Role
    $('#role_datatable').on('click', '.btn-delete', function () {
        const roleId = $(this).data('id');
        const canDelete = $(this).data('candelete');

        // Validasi: hanya bisa delete jika inactive
        if (!canDelete) {
            Swal.fire({
                title: 'Cannot Delete Active Role',
                text: 'Please deactivate the role first before deleting',
                icon: 'warning',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-warning'
                },
                buttonsStyling: false
            });
            return;
        }

        Swal.fire({
            title: 'Delete selected data?',
            text: "Are you sure you want to delete this role? This action cannot be undone.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, Delete',
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
                        Swal.fire({
                            title: 'Deleted!',
                            text: response.message,
                            icon: 'success',
                            confirmButtonText: 'OK',
                            customClass: {
                                confirmButton: 'btn btn-primary'
                            },
                            buttonsStyling: false
                        });
                        table.ajax.reload();
                    }
                } catch (error) {
                    Swal.fire({
                        title: 'Error',
                        text: error.responseJSON?.message || 'Failed to delete role',
                        icon: 'error',
                        confirmButtonText: 'OK',
                        customClass: {
                            confirmButton: 'btn btn-danger'
                        },
                        buttonsStyling: false
                    });
                }
            }
        });
    });
});
