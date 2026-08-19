$(document).ready(function () {
    const currentRoleId = $('#role-id').val();

    // Load permissions on page load
    loadMenuPermissions(currentRoleId);

    // Save Permissions
    $('#btn-save-permissions').on('click', async function () {
        const permissions = collectPermissions();

        try {
            const response = await $.ajax({
                url: '/Role/SaveRoleMenuPermissions',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    roleId: currentRoleId,
                    permissions: permissions
                })
            });

            if (response.success) {
                Swal.fire({
                    title: 'Success',
                    text: 'Permissions saved successfully',
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
                text: error.responseJSON?.message || 'Failed to save permissions',
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
