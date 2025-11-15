$(document).ready(function () {
    console.log('load script detail approval');

    $('#btn-download-pdf').click(function () {
        var invoiceId = $(this).data('id');
        
        // Show loading
        Swal.fire({
            title: 'Generating PDF...',
            text: 'Please wait',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Download PDF
        window.location.href = `/InvoiceReview/DownloadPdf?id=${invoiceId}`;
        
        // Close loading after a delay
        setTimeout(() => {
            Swal.close();
        }, 2000);
    });

    $('#btn-approve-invoice').click(function () {
        var invoiceId = $(this).data('id');
        
        Swal.fire({
            title: 'Confirmation',
            text: "Are you sure you want to approve this data ?",
            showCancelButton: true,
            confirmButtonText: 'Approve',
            cancelButtonText: 'Cancel',
            reverseButtons: true, // Swap positions of confirm & cancel
            customClass: {
                confirmButton: 'btn btn-primary', // custom CSS class for confirm
                cancelButton: 'btn button-outline-blue-komatsu' // custom CSS class for cancel
            },
            buttonsStyling: false // Allow custom Bootstrap classes
        }).then((result) => {
            if (result.isConfirmed) {
                // Ajax call to approve
                var invoiceIds = [];
                invoiceIds.push(invoiceId);

                $.ajax({
                    url: `/InvoiceReview/Approve`,
                    type: 'patch',
                    contentType: 'application/json',
                    data: JSON.stringify({ id: invoiceIds }),
                    success: function (response) {
                        Swal.fire({
                            icon: "success",
                            title: "Successfully",
                            text: "Selected data successfully approved!",
                            timer: 2000,
                            willClose: () => {
                                window.location.href = '/InvoiceReview';
                            }
                        });
                    },
                    error: function () {
                        Swal.fire(
                            'Error!',
                            'Something went wrong while approve.',
                            'error'
                        );
                    }
                });
            }
        });
    });

    $('#btn-reject-invoice').click(function () {
        var invoiceId = $(this).data('id');
        
        Swal.fire({
            title: 'Confirmation',
            text: "Do you want to return this data for revision ?",
            input: 'textarea',
            inputPlaceholder: 'Reason',
            inputAttributes: {
                'aria-label': 'Reason'
            },
            showCancelButton: true,
            confirmButtonText: 'Reject',
            cancelButtonText: 'Cancel',
            reverseButtons: true, // Swap positions of confirm & cancel
            customClass: {
                confirmButton: 'btn btn-danger', // custom CSS class for confirm
                cancelButton: 'btn button-outline-blue-komatsu' // custom CSS class for cancel
            },
            buttonsStyling: false // Allow custom Bootstrap classes
        }).then((result) => {
            if (result.isConfirmed) {
                // Ajax call to reject
                var invoiceIds = [];
                invoiceIds.push(invoiceId);

                $.ajax({
                    url: `/InvoiceReview/Reject`,
                    type: 'patch',
                    contentType: 'application/json',
                    data: JSON.stringify({ id: invoiceIds, note: result.value }),
                    success: function (response) {
                        Swal.fire({
                            icon: "success",
                            title: "Successfully",
                            text: "Selected data successfully revise!",
                            timer: 2000,
                            willClose: () => {
                                window.location.href = '/InvoiceReview';
                            }
                        });
                    },
                    error: function () {
                        Swal.fire(
                            'Error!',
                            'Something went wrong while revise.',
                            'error'
                        );
                    }
                });
            }
        });
    });
});