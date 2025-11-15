$(document).ready(function () {
    $("#btn-delete-unit").on("click", function (e) {
        e.preventDefault();

        var registerUnitId = $("#unit-data").data("id");
        var status = $("#unit-data").data("status");
        console.log(`status ${registerUnitId}`)
        console.log(`status ${status}`)

        if (status !== 5) {
            Swal.fire({
                title: 'Data cannot be deleted',
                text: 'Data cannot be deleted because its current status does not allow deletion',
                icon: 'warning',
                confirmButtonText: 'OK',
                customClass: {
                    confirmButton: 'btn btn-primary'
                },
                buttonsStyling: false
            });
            return; // stop disini
        }
        Swal.fire({
            title: 'Delete selected data?',
            text: "Are you sure you want to delete this data?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Delete',
            reverseButtons: true, // Swap positions of confirm & cancel
            customClass: {
                confirmButton: 'btn btn-danger', // custom CSS class for confirm
                cancelButton: 'btn button-outline-blue-komatsu' // custom CSS class for cancel
            },
            buttonsStyling: false // Allow custom Bootstrap classes
        })
            .then((result) => {
                if (result.isConfirmed) {
                    // Ajax call to delete
                    $.ajax({
                        url: `/RegisterUnit/Deleted`,
                        type: 'DELETE',
                        contentType: 'application/json',
                        data: JSON.stringify(registerUnitId),
                        success: function (response) {
                            Swal.fire(
                                'Successfully',
                                'Selected data successfully deleted',
                                'success'
                            )
                            .then(() => {
                                window.location.href = "/RegisterUnit";
                            });
                        },
                        error: function () {
                            Swal.fire(
                                'Error!',
                                'Something went wrong while deleting.',
                                'error'
                            );
                        }
                    });
                }
            });
    });
});