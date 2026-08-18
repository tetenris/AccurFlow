$(document).ready(function () {
    let isEditMode = false;
    let currentUserId = null;

    // Load Role Dropdown
    function loadRoleDropdown(targetSelect) {
        return $.get('/User/GetRoleDropdown', function (data) {
            const $select = $(targetSelect);
            $select.empty();
            
            if (targetSelect === '#filter-role') {
                $select.append('<option value="">All Roles</option>');
            } else {
                $select.append('<option value="">Select Role</option>');
            }
            
            $.each(data, function (index, item) {
                $select.append($('<option>', {
                    value: item.value,
                    text: item.text
                }));
            });
        });
    }

    // Load dropdowns on page load
    loadRoleDropdown('#user-role');
    loadRoleDropdown('#filter-role');

    // Initialize DataTable
    const table = $("#user_datatable").DataTable({
        serverSide: true,
        searching: true,
        scrollX: false,
        pageLength: 10,
        responsive: true,
        dom: '<"top"lf>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search'
        },
        initComplete: function (settings, json) {
            if ($('#user_datatable_length').find('span.mr-2').length === 0) {
                $('#user_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            // Add search icon to filter
            $('#user_datatable_filter').css('position', 'relative');
            $('#user_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');
            
            // Style search box to match status dropdown
            $('#user_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            // Style show dropdown to match status dropdown
            $('#user_datatable_length select').css({
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
            url: '/User/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const isActive = $('#filter-status').val();
                const roleId = $('#filter-role').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    IsActive: isActive === "" ? null : isActive === "true",
                    RoleId: roleId === "" ? null : roleId
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
            { data: "userName" },
            { data: "email" },
            { data: "fullName" },
            { data: "roleName" },
            {
                data: "isActive",
                render: function (data, type, row) {
                    if (row.isLocked) {
                        return '<span class="badge badge-light-danger">Locked</span>';
                    }

                    return data
                        ? '<span class="badge badge-light-success">Active</span>'
                        : '<span class="badge badge-light-warning">Inactive</span>';
                }
            },
            {
                data: "userId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    const unlockButton = row.isLocked
                        ? `<button class="btn btn-sm btn-icon btn-light-info btn-unlock" data-id="${data}" title="Unlock Password">
                                <i class='fa fa-unlock'></i>
                           </button>`
                        : '';

                    return `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-primary btn-detail" data-id="${data}" title="View Detail">
                                <i class='fa fa-eye'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}" title="Edit">
                                <i class='fa fa-pencil'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-danger btn-delete" data-id="${data}" data-candelete="${!row.isActive}" title="Delete">
                                <i class='fa fa-trash-alt'></i>
                            </button>
                            ${unlockButton}
                            <button class="btn btn-sm btn-icon btn-light-warning btn-audit" 
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

    // Add button to toolbar
    var btnAdd = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-add-user',
        html: '<i class="fa fa-add"></i> Add New User'
    });
    $('#toolbar-section-button').append(btnAdd);

    // Add User
    $(document).on('click', '#btn-add-user', function () {
        isEditMode = false;
        currentUserId = null;
        $('#modal-user-title').text('Add User');
        $('#form-user')[0].reset();
        $('#user-role').val('');
        $('#user-active').prop('checked', true);

        $('#modal-user').modal('show');
    });

    // Edit User
    $('#user_datatable').on('click', '.btn-edit', async function () {
        const userId = $(this).data('id');
        isEditMode = true;
        currentUserId = userId;

        try {
            await loadRoleDropdown('#user-role');
            const response = await $.get(`/User/GetById?id=${userId}`);

            $('#modal-user-title').text('Edit User');
            $('#user-id').val(response.userId);
            $('#user-username').val(response.userName);
            $('#user-email').val(response.email);
            $('#user-fullname').val(response.fullName);
            $('#user-role').val(response.roleId);
            $('#user-active').prop('checked', response.isActive);
            
            $('#modal-user').modal('show');
        } catch (error) {
            Swal.fire({
                title: 'Error',
                text: 'Failed to load user data',
                icon: 'error',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
        }
    });

    // Save User
    $('#btn-save-user').on('click', async function () {
        const userName = $('#user-username').val().trim();
        const email = $('#user-email').val().trim();
        const fullName = $('#user-fullname').val().trim();
        const roleId = $('#user-role').val();

        if (!userName) {
            Swal.fire('Warning', 'Username is required', 'warning');
            return;
        }

        if (!email) {
            Swal.fire('Warning', 'Email is required', 'warning');
            return;
        }

        if (!fullName) {
            Swal.fire('Warning', 'Full name is required', 'warning');
            return;
        }

        if (!roleId) {
            Swal.fire('Warning', 'Role is required', 'warning');
            return;
        }

        const data = {
            UserName: userName,
            Email: email,
            FullName: fullName,
            RoleId: roleId,
            IsActive: $('#user-active').is(':checked')
        };

        if (isEditMode) {
            data.UserId = currentUserId;
        }

        try {
            const url = isEditMode ? '/User/Edit' : '/User/Create';
            const response = await $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            });

            if (response.success) {
                Swal.fire({
                    title: 'Success',
                    text: response.message,
                    icon: 'success',
                    confirmButtonText: 'OK',
                    customClass: {
                        confirmButton: 'btn btn-success'
                    },
                    buttonsStyling: false
                });
                $('#modal-user').modal('hide');
                table.ajax.reload();
            }
        } catch (error) {
            Swal.fire({
                title: 'Error',
                text: error.responseJSON?.message || 'Failed to save user',
                icon: 'error',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
        }
    });

    // Detail User
    $('#user_datatable').on('click', '.btn-detail', async function () {
        const userId = $(this).data('id');

        try {
            const response = await $.get(`/User/GetById?id=${userId}`);
            const statusText = response.isActive ? 'Active' : 'Inactive';
            const lockStatus = response.isLocked ? 'Locked' : 'Unlocked';

            Swal.fire({
                title: 'User Details',
                html: `
                    <div class="text-start">
                        <table class="table table-row-bordered">
                            <tr>
                                <td class="fw-bold" width="40%">Username</td>
                                <td>${response.userName}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Email</td>
                                <td>${response.email}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Full Name</td>
                                <td>${response.fullName}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Role</td>
                                <td>${response.roleName}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Status</td>
                                <td>${statusText}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Password Lock</td>
                                <td>${lockStatus}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Failed Login</td>
                                <td>${response.failedLoginAttempts || 0}</td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Password Expires At</td>
                                <td>${response.passwordExpiresAt ? moment(response.passwordExpiresAt).format('DD-MMM-YYYY HH:mm') : '-'}</td>
                            </tr>
                            ${response.lockedAt ? `
                            <tr>
                                <td class="fw-bold">Locked At</td>
                                <td>${moment(response.lockedAt).format('DD-MMM-YYYY HH:mm')}</td>
                            </tr>` : ''}
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
                text: 'Failed to load user details',
                icon: 'error',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
        }
    });

    // Unlock User
    $('#user_datatable').on('click', '.btn-unlock', function () {
        const userId = $(this).data('id');

        Swal.fire({
            title: 'Unlock password?',
            text: 'Password user akan direset ke default Qwerty@123 dan user wajib ganti password saat login berikutnya.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Unlock',
            cancelButtonText: 'Cancel',
            customClass: {
                confirmButton: 'btn btn-info',
                cancelButton: 'btn btn-secondary'
            },
            buttonsStyling: false
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    const response = await $.ajax({
                        url: '/User/Unlock',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(userId)
                    });

                    if (response.success) {
                        Swal.fire({
                            title: 'Unlocked!',
                            text: response.message,
                            icon: 'success',
                            confirmButtonText: 'OK',
                            customClass: {
                                confirmButton: 'btn btn-success'
                            },
                            buttonsStyling: false
                        });
                        table.ajax.reload();
                    }
                } catch (error) {
                    Swal.fire({
                        title: 'Error',
                        text: error.responseJSON?.message || 'Failed to unlock user',
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

    // Audit Trail
    $('#user_datatable').on('click', '.btn-audit', function () {
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

    // Delete User
    $('#user_datatable').on('click', '.btn-delete', function () {
        const userId = $(this).data('id');
        const canDelete = $(this).data('candelete');

        if (!canDelete) {
            Swal.fire({
                title: 'Cannot Delete Active User',
                text: 'Please deactivate the user first before deleting',
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
            text: "Are you sure you want to delete this user? This action cannot be undone.",
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
                        url: '/User/Delete',
                        type: 'DELETE',
                        contentType: 'application/json',
                        data: JSON.stringify(userId)
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
                        text: error.responseJSON?.message || 'Failed to delete user',
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
