$(document).ready(function () {
    let currentRoleId = $('#role-id').val();

    // Populate Role Type dropdown from shared data
    function populateRoleType() {
        const $select = $('#role-type');
        $select.empty();
        const currentRoleType = $('#role-type-value').val();
        $.each(window.roleTypeData, function (key, item) {
            $select.append($('<option>', {
                value: key,
                text: item.name,
                selected: key === String(currentRoleType)
            }));
        });
    }

    // Load dropdown + role data + permissions on page load
    loadMenuPermissions(currentRoleId);
    populateRoleType();

    // Save Role
    $('#btn-save-role').on('click', async function () {
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
            RoleId: currentRoleId,
            RoleType: parseInt(roleType),
            RoleName: roleName,
            Description: $('#role-description').val(),
            IsActive: $('#role-active').is(':checked')
        };

        try {
            const response = await $.ajax({
                url: '/Role/Edit',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            });

            if (response.success) {
                // Save permissions
                const permissions = collectPermissions();

                await $.ajax({
                    url: '/Role/SaveRoleMenuPermissions',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        roleId: currentRoleId,
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
                }).then(function () {
                    window.location.href = '/Role/Index';
                });
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
});