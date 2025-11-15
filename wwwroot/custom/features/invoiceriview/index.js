var selectedInvoiceReviewIds = [];

$(document).ready(function () {

    var btnDownload = $('<a>', {
        href: "/InvoiceReview/DownloadPdf",
        class: 'btn button-outline-blue-komatsu',
        type: 'button',
        id: 'btn-download-billing',
        html: `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd"
                d="M5.16659 15.1665C4.28253 15.1665 3.43468 14.8153 2.80956 14.1902C2.18444 13.5651 1.83325 12.7172 1.83325 11.8332V9.83317C1.83325 9.65636 1.90349 9.48679 2.02851 9.36177C2.15354 9.23674 2.32311 9.1665 2.49992 9.1665C2.67673 9.1665 2.8463 9.23674 2.97132 9.36177C3.09635 9.48679 3.16659 9.65636 3.16659 9.83317V11.8332C3.16659 12.3636 3.3773 12.8723 3.75237 13.2474C4.12744 13.6225 4.63615 13.8332 5.16659 13.8332H11.8333C12.3637 13.8332 12.8724 13.6225 13.2475 13.2474C13.6225 12.8723 13.8333 12.3636 13.8333 11.8332V9.83317C13.8333 9.65636 13.9035 9.48679 14.0285 9.36177C14.1535 9.23674 14.3231 9.1665 14.4999 9.1665C14.6767 9.1665 14.8463 9.23674 14.9713 9.36177C15.0963 9.48679 15.1666 9.65636 15.1666 9.83317V11.8332C15.1666 12.7172 14.8154 13.5651 14.1903 14.1902C13.5652 14.8153 12.7173 15.1665 11.8333 15.1665H5.16659Z"
                fill="#18A4F5"/>
            <path fill-rule="evenodd" clip-rule="evenodd"
                d="M5.0235 7.76642C4.96221 7.82904 4.91387 7.90312 4.88123 7.98443C4.84859 8.06574 4.8323 8.15269 4.83329 8.2403C4.83429 8.32791 4.85254 8.41447 4.887 8.49502C4.92147 8.57558 4.97148 8.64854 5.03417 8.70976L8.03417 11.6431C8.15873 11.7648 8.32599 11.833 8.50017 11.833C8.67435 11.833 8.84161 11.7648 8.96617 11.6431L11.9662 8.70976C12.0301 8.64887 12.0813 8.5759 12.1168 8.49507C12.1523 8.41424 12.1714 8.32715 12.173 8.23888C12.1746 8.15061 12.1586 8.0629 12.126 7.98085C12.0934 7.89879 12.0449 7.82403 11.9832 7.7609C11.9214 7.69777 11.8478 7.64753 11.7665 7.61309C11.6852 7.57865 11.5979 7.5607 11.5096 7.56028C11.4213 7.55987 11.3338 7.57699 11.2522 7.61066C11.1706 7.64433 11.0965 7.69388 11.0342 7.75642L9.16684 9.58242V3.83309C9.16684 3.65628 9.0966 3.48671 8.97157 3.36168C8.84655 3.23666 8.67698 3.16642 8.50017 3.16642C8.32336 3.16642 8.15379 3.23666 8.02877 3.36168C7.90374 3.48671 7.8335 3.65628 7.8335 3.83309V9.58242L5.96684 7.75642C5.90422 7.69513 5.83014 7.64679 5.74883 7.61415C5.66752 7.58151 5.58057 7.56522 5.49296 7.56621C5.40535 7.5672 5.31879 7.58545 5.23824 7.61992C5.15768 7.65439 5.08471 7.7044 5.0235 7.76709V7.76642Z"
                fill="#18A4F5"/>
        </svg> Download PDF`
    });



    // Tombol Import
    const btnImport = $('<button>', {
        class: 'btn button-outline-blue-komatsu me-2',
        type: 'button',
        id: 'btn-import',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M12 2V15M12 15L8 11M12 15L16 11M4 20H20" 
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Import`
    });

    // Tombol Approval History
    const btnApprovalHistory = $('<button>', {
        class: 'btn button-outline-blue-komatsu me-2',
        type: 'button',
        id: 'btn-approval-history',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M12 8V12L15 15M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z" 
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Approval History`
    });

    // Tombol Resend
    const btnResend = $('<button>', {
        class: 'btn button-outline-blue-komatsu me-2',
        type: 'button',
        id: 'btn-resend',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M3 12L7 8M3 12L7 16M3 12H15M21 12L17 16M21 12L17 8M21 12H9" 
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Resend`
    });

    // Tombol Edit
    const btnEdit = $('<button>', {
        class: 'btn button-outline-blue-komatsu me-2',
        type: 'button',
        id: 'btn-edit',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M11 4H4C3.46957 4 2.96086 4.21071 2.58579 4.58579C2.21071 4.96086 2 5.46957 2 6V20C2 20.5304 2.21071 21.0391 2.58579 21.4142C2.96086 21.7893 3.46957 22 4 22H18C18.5304 22 19.0391 21.7893 19.4142 21.4142C19.7893 21.0391 20 20.5304 20 20V13" 
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M18.5 2.50001C18.8978 2.10219 19.4374 1.87869 20 1.87869C20.5626 1.87869 21.1022 2.10219 21.5 2.50001C21.8978 2.89784 22.1213 3.4374 22.1213 4.00001C22.1213 4.56262 21.8978 5.10219 21.5 5.50001L12 15L8 16L9 12L18.5 2.50001Z" 
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Edit`
    });

    // Tombol Export
    const btnExport = $('<button>', {
        class: 'btn button-outline-blue-komatsu me-2',
        type: 'button',
        id: 'btn-export',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M12 22V9M12 9L8 13M12 9L16 13M4 4H20"
                    stroke="#18A4F5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Export`
    });

    // Tombol Detail (sesuai contoh kamu)
    const btnDetail = $('<button>', {
        class: 'btn button-outline-blue-komatsu',
        type: 'button',
        id: 'detail-invoice',
        html: `
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none"
                xmlns="http://www.w3.org/2000/svg" class="me-2">
                <path d="M5 3H14L19 8V21H5V3Z" stroke="#18A4F5" stroke-width="1.5" 
                    stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M14 3V8H19" stroke="#18A4F5" stroke-width="1.5" 
                    stroke-linecap="round" stroke-linejoin="round"/>
            </svg> Detail`
    });

    if (window.location.pathname === "/InvoiceReview/DetailStaff") {
        $('#toolbar-section-button').append(btnDownload);
    }
    else {
        $('#toolbar-section-button').append(btnImport, btnApprovalHistory, btnResend, btnEdit, btnExport, btnDetail);
    }

    $('#btn-approval-history').on('click', function () {
        let invoiceId = $('#invoice-id').val();

        if (!invoiceId) {
            invoiceId = crypto.randomUUID();
            console.log('Generated dynamic InvoiceId for approval history:', invoiceId);
        }

        $.ajax({
            url: '/InvoiceReview/GetApprovalHistory',
            type: 'GET',
            data: { invoiceId: invoiceId },
            success: function (res) {
                if (!res || res.length === 0) {
                    $('#approvalHistoryBody').html('<tr><td colspan="8" class="text-center">No approval history found</td></tr>');
                    $('#approvalHistoryModal').modal('show');
                    return;
                }

                let html = '';
                res.forEach((item, index) => {
                    const statusBadge = item.status === 'Approved' 
                        ? '<span class="badge badge-light-success">Approved</span>' 
                        : '<span class="badge badge-light-danger">Rejected</span>';
                    
                    html += `
                        <tr>
                            <td>${index + 1}</td>
                            <td>${item.dateTime ? moment(item.dateTime).format('DD-MMM-YYYY HH:mm') : '-'}</td>
                            <td>${item.approverName ?? '-'}</td>
                            <td>${item.division ?? '-'}</td>
                            <td>${item.organization ?? '-'}</td>
                            <td class="text-center">${item.level ?? '-'}</td>
                            <td class="text-center">${statusBadge}</td>
                            <td>${item.notes ?? '-'}</td>
                        </tr>
                    `;
                });

                $('#approvalHistoryBody').html(html);
                $('#approvalHistoryModal').modal('show');
            },
            error: function (err) {
                console.error(err);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Failed to load approval history'
                });
            }
        });
    });

    $('#btn-import').on('click', function () {
        // Ambil invoiceId dari halaman Detail
        let invoiceId = $('#invoice-id').val();

        // Jika tidak ada invoiceId, generate GUID dinamis untuk testing
        if (!invoiceId) {
            invoiceId = crypto.randomUUID(); // Generate GUID dinamis
            console.log('Generated dynamic InvoiceId:', invoiceId);
        }

        // Buat input file dinamis
        const fileInput = $('<input>', {
            type: 'file',
            accept: '.xlsx,.xls,.pdf',
            style: 'display: none'
        });

        // Saat file dipilih
        fileInput.on('change', function (e) {
            const file = e.target.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('file', file);
            formData.append('invoiceId', invoiceId);

            // Upload ke controller
            $.ajax({
                url: '/InvoiceReview/Upload',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    Swal.fire({
                        icon: 'success',
                        icon: 'success',
                        title: 'Upload berhasil!',
                        text: `File ${res.fileName} berhasil diupload dan disimpan ke database`
                    });
                },
                error: function (xhr) {
                    var errorMessage = 'Terjadi kesalahan saat upload file.';

                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        try {
                            var response = JSON.parse(xhr.responseText);
                            errorMessage = response.message || errorMessage;
                        } catch (e) {
                            errorMessage = xhr.responseText;
                        }
                    }

                    Swal.fire({
                        icon: 'error',
                        title: 'Gagal upload',
                        text: errorMessage
                    });
                }
            });
        });

        // Trigger klik input file
        fileInput.click();
    });


    $('#btn-export').on('click', function () {
        // Ambil invoiceId dari halaman Detail
        let invoiceId = $('#invoice-id').val();

        // Jika tidak ada invoiceId, generate GUID dinamis untuk testing
        if (!invoiceId) {
            invoiceId = crypto.randomUUID(); // Generate GUID dinamis
            console.log('Generated dynamic InvoiceId for export:', invoiceId);
        }

        Swal.fire({
            title: 'Exporting...',
            text: 'Please wait while we generate the Excel file',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => Swal.showLoading()
        });

        // Download Excel file
        window.location.href = `/InvoiceReview/ExportDetailUnit?invoiceId=${invoiceId}`;

        // Close loading after a short delay
        setTimeout(() => {
            Swal.close();
            Swal.fire({
                icon: 'success',
                title: 'Export berhasil!',
                text: 'File Excel berhasil didownload',
                timer: 2000,
                showConfirmButton: false
            });
        }, 1500);
    });

    $('#btn-resend').on('click', function () {
        // Ambil invoiceId dari halaman Detail
        let invoiceId = $('#invoice-id').val();

        // Jika tidak ada invoiceId, generate GUID dinamis untuk testing
        if (!invoiceId) {
            invoiceId = crypto.randomUUID(); // Generate GUID dinamis
            console.log('Generated dynamic InvoiceId for resend:', invoiceId);
        }

        Swal.fire({
            title: 'Sending...',
            text: 'Please wait',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => Swal.showLoading()
        });

        // Kirim request resend
        $.ajax({
            url: '/InvoiceReview/ResendInvoice',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ invoiceId: invoiceId }),
            success: function (response) {
                Swal.close();

                // Format email inbox style
                const emailHtml = `
                    <div style="text-align: left; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background: #f9f9f9;">
                        <div style="margin-bottom: 15px;">
                            <strong>From:</strong> Invoice System <noreply@komatsu.com>
                        </div>
                        <div style="margin-bottom: 15px;">
                            <strong>To:</strong> Finance Department
                        </div>
                        <div style="margin-bottom: 15px;">
                            <strong>Subject:</strong> Invoice Document - Resend
                        </div>
                        <hr style="border: 1px solid #ddd; margin: 15px 0;">
                        <div style="margin-bottom: 20px;">
                            <p>Dear Finance Team,</p>
                            <p>Please find the attached invoice document that has been resent as requested.</p>
                            <p>This is an automated message from the Invoice Review System.</p>
                            <p>Best regards,<br>Invoice System</p>
                        </div>
                        <div style="background: #fff; padding: 10px; border: 1px solid #ddd; border-radius: 4px;">
                            <strong>📎 Attachment:</strong>
                            <a href="#" id="download-attachment" style="color: #18A4F5; text-decoration: none; margin-left: 10px;">
                                ${response.fileName}
                            </a>
                        </div>
                    </div>
                `;

                Swal.fire({
                    title: 'Invoice Sent Successfully',
                    html: emailHtml,
                    icon: 'success',
                    width: 600,
                    confirmButtonText: 'Close',
                    didOpen: () => {
                        // Handle download attachment
                        $('#download-attachment').on('click', function (e) {
                            e.preventDefault();

                            // Convert base64 to blob and download
                            const byteCharacters = atob(response.fileData);
                            const byteNumbers = new Array(byteCharacters.length);
                            for (let i = 0; i < byteCharacters.length; i++) {
                                byteNumbers[i] = byteCharacters.charCodeAt(i);
                            }
                            const byteArray = new Uint8Array(byteNumbers);
                            const blob = new Blob([byteArray], { type: 'application/pdf' });

                            const link = document.createElement('a');
                            link.href = window.URL.createObjectURL(blob);
                            link.download = response.fileName;
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);
                        });
                    }
                });
            },
            error: function (xhr) {
                Swal.close();
                var errorMessage = 'Terjadi kesalahan saat mengirim invoice.';

                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }

                Swal.fire({
                    icon: 'error',
                    title: 'Gagal mengirim',
                    text: errorMessage
                });
            }
        });
    });

    $('#btn-edit').on('click', function () {
        // Ambil invoiceId dari halaman Detail
        let invoiceId = $('#invoice-id').val();

        // Jika tidak ada invoiceId, generate GUID dinamis untuk testing
        if (!invoiceId) {
            invoiceId = crypto.randomUUID(); // Generate GUID dinamis
            console.log('Generated dynamic InvoiceId for edit:', invoiceId);
        }

        // Redirect ke halaman Detail untuk edit
        window.location.href = `/InvoiceReview/Detail?id=${invoiceId}`;
    });

    $('#detail-invoice').on('click', function () {
        window.location.href = '/InvoiceReview/DetailStaff';
    });

    loadInvoiceReviewTable();

    $('#applyFilter-invoice').on('click', function () {
        $('#invoiceReviewTable').DataTable().ajax.reload();
    });

    $('#resetFilter-invoice').on('click', function () {
        $('#filter-status-invoice').val('').trigger('change');
        $('#invoiceReviewTable').DataTable().ajax.reload();
    });
});



function loadInvoiceReviewTable() {
    // Hapus DataTable lama kalau ada
    if ($.fn.DataTable.isDataTable('#invoice_table')) {
        $('#invoice_table').DataTable().destroy();
    }

    // Inject HTML table baru ke dalam div container
    const tableHtml = `
        <table id="invoice_table" class="table table-striped table-row-bordered gy-5 gs-7 border rounded w-100">
            <thead>
                <tr>
                    <th><input type="checkbox" id="checkAll" class="cbx-all"></th>
                    <th>No</th>
                    <th>Created Date</th>
                    <th>Invoice Number</th>
                    <th>Invoice Date</th>
                    <th>Due Date</th>
                    <th>Register Period</th>
                    <th>Description</th>
                    <th>Billing To</th>
                    <th>Status</th>
                    <th>Unit</th>
                    <th>Action</th>
                </tr>
            </thead>
        </table>
    `;
    $("#invoiceReviewTable").html(tableHtml);

    // Inisialisasi DataTable di elemen <table> yang baru dibuat
    $("#invoice_table").DataTable({
        serverSide: true,
        searching: true,
        scrollX: true,
        pageLength: 10,
        responsive: true,
        "dom": '<"top d-flex align-items-center justify-content-between"<"d-flex" lf>>rt<"bottom d-flex align-items-center justify-content-between"ip><"clear">',
        language: {
            search: '',
            searchPlaceholder: 'Search',
            emptyTable: "No data displayed",
            infoEmpty: "No entries to show"
        },
        ajax: {
            url: '/InvoiceReview/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    Status: $('#filter-status-invoice').val() || null
                });
            }
        },
        columns: [
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {

                    var disabled = row.status != "NeedReview" || row.status == "InReview" ? "" : "disabled";

                    return `<input type="checkbox" class="cbx-item" value="${row.invoiceReviewId}" ${disabled}'>`;
                }
            },
            { data: null, render: (d, t, r, m) => m.row + m.settings._iDisplayStart + 1 },
            { data: "createdDate", render: d => d ? moment(d).format("DD MMM YYYY") : "-" },
            { data: "invoiceNumber", defaultContent: "-" },
            { data: "invoiceDate", render: d => d ? moment(d).format("DD MMM YYYY") : "-" },
            { data: "dueDate", render: d => d ? moment(d).format("DD MMM YYYY") : "-" },
            { data: "registerPeriod", defaultContent: "-" },
            { data: "description", defaultContent: "-" },
            { data: "billingTo", defaultContent: "-" },
            {
                data: "status",
                render: data => {
                    switch (data) {
                        case "NeedReview": return `<span class="badge bg-warning text-dark">Need Review</span>`;
                        case "InReview": return `<span class="badge bg-info text-dark">In Review</span>`;
                        case "Submitted": return `<span class="badge bg-primary">Submitted</span>`;
                        case "Done": return `<span class="badge bg-success">Done</span>`;
                        case "Rejected": return `<span class="badge bg-danger">Rejected</span>`;
                        case "Sent": return `<span class="badge bg-secondary">Sent</span>`;
                        default: return `<span class="badge bg-light text-muted">Unknown</span>`;
                    }
                }
            },
            {
                data: null,
                orderable: false,
                className: "text-center",
                render: function (data, type, row) {
                    return `
                            <button class="btn btn-sm btn-outline-primary btn-detail" 
                                data-created="${row.createdDate}">
                                <i class="bi bi-eye"></i>
                            </button>`;
                }
            },
            {
                data: null,
                orderable: false,
                className: "text-center",
                render: row => `
                    <button class="btn btn-sm btn-outline-primary" data-id="${row.invoiceReviewId}">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-success" data-id="${row.invoiceReviewId}">
                        <i class="bi bi-pencil"></i>
                    </button>`
            }
        ],
        order: [[1, 'desc']],
        initComplete: function () {
            if ($('#invoice_table_length').find('span.mr-2').length === 0) {
                $('#invoice_table_length').prepend('<span class="mr-2">Show</span>');
            }
        }
    });


    //Event: Klick checkbox all and item
    $('#checkAll').on('click', function () {
        let checked = this.checked;
        selectedInvoiceReviewIds = []; // reset first

        $('.cbx-item').each(function () {
            let $cbx = $(this);
            let isComplete = $cbx.data('iscomplete');
            let val = $cbx.val();

            if (checked) {
                // Only check items with isComplete === true
                if (isComplete) {
                    $cbx.prop('checked', true);
                    if (!selectedInvoiceReviewIds.includes(val)) {
                        selectedInvoiceReviewIds.push(val);
                    }
                } else {
                    $cbx.prop('checked', false); // ensure unchecked if not complete
                }
            } else {
                // Uncheck all when master is unchecked
                $cbx.prop('checked', false);
            }
        });

        // Enable/disable button based on whether there are any selected IDs
        // $('#btn-bulk-approval').prop('disabled', selectedInvoiceReviewIds.length === 0);
    });

    $('#invoice_table tbody').on('click', '.cbx-item', function () {
        let val = $(this).val();

        if ($(this).is(':checked')) {
            if (!selectedInvoiceReviewIds.includes(val)) {
                selectedInvoiceReviewIds.push(val);
            }
            // $('#btn-bulk-approval').prop('disabled', false)
        } else {
            selectedInvoiceReviewIds = selectedInvoiceReviewIds.filter(x => x !== val);
            // $('#btn-bulk-approval').prop('disabled', true)
        }

        $('#checkAll').prop(
            'checked',
            $('.cbx-item:checked').length === $('.cbx-item').length
        );
    });

    /* Bulk Approval - Commented out
    $(document).on('click', '#btn-bulk-approval', function (e) {
        e.preventDefault();
        Swal.fire({
            title: 'Approval',
            text: "Change Status To",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: "#18A4F5",
            confirmButtonText: 'Approval',
            cancelButtonText: 'Reject',
            reverseButtons: false,
            customClass: {
                confirmButton: 'btn btn-primary',
                cancelButton: 'btn btn-danger'
            },
            buttonsStyling: false
        }).then((result) => {
            if (result.isConfirmed) { //approve
                Swal.fire({
                    title: 'Confirmation',
                    text: "Are you sure you want to approve this data?",
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: "#18A4F5",
                    confirmButtonText: 'Approve',
                    cancelButtonText: 'Cancel',
                    reverseButtons: true,
                    customClass: {
                        confirmButton: 'btn btn-primary',
                        cancelButton: 'btn button-outline-blue-komatsu'
                    },
                    buttonsStyling: false
                }).then((result) => {
                    if (result.isConfirmed) {

                        var data = {
                            id: selectedInvoiceReviewIds
                        }

                        $.ajax({
                            url: `/InvoiceReview/Approve`,
                            type: "POST",
                            dataType: "json",
                            contentType: 'application/json; charset=utf-8',
                            data: JSON.stringify(data),
                            success: function (res) {
                                Swal.fire({
                                    icon: "success",
                                    title: "Successfully",
                                    text: "Units successfully delivered to Register!",
                                    timer: 2000,
                                    willClose: () => {
                                        window.location.href = '/InvoiceReview';
                                    }
                                });
                            },
                            error: function (xhr) {
                                Swal.fire({
                                    icon: "error",
                                    title: "Error",
                                    text: xhr.responseText || "Something went wrong!"
                                });
                            }
                        });
                    }
                });
            }
            else if (result.dismiss === Swal.DismissReason.cancel) { //reject
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

                        var data = {
                            id: selectedInvoiceReviewIds,
                            note: result.value
                        }

                        $.ajax({
                            url: `/InvoiceReview/Reject`,
                            type: "POST",
                            dataType: "json",
                            contentType: 'application/json; charset=utf-8',
                            data: JSON.stringify(data),
                            success: function (res) {
                                Swal.fire({
                                    icon: "success",
                                    title: "Successfully",
                                    text: "Units successfully delivered to Register!",
                                    timer: 2000,
                                    willClose: () => {
                                        window.location.href = '/InvoiceReview';
                                    }
                                });
                            },
                            error: function (xhr) {
                                Swal.fire({
                                    icon: "error",
                                    title: "Error",
                                    text: xhr.responseText || "Something went wrong!"
                                });
                            }
                        });
                    }
                });
            }
        });
    });
    */

    // Event: Klik tombol detail di tabel
    $(document).on('click', '.btn-detail', function () {
        const id = $(this).data('id');

        $.ajax({
            url: `/InvoiceReview/GetDetailInvoiceUnit`,
            type: 'GET',
            data: { checkDate: id }, // misalnya kalau kamu mau ambil by tanggal, tapi bisa ganti ke invoiceId
            success: function (res) {
                if (!res || res.length === 0) {
                    $('#invoiceDetailBody').html('<tr><td colspan="2" class="text-center">No details found</td></tr>');
                    $('#invoiceDetailModal').modal('show');
                    return;
                }

                // Kalau hasilnya array, ambil item pertama
                const item = Array.isArray(res) ? res[0] : res;

                let html = `
                <tr><th>Invoice ID</th><td>${item.invoiceId ?? '-'}</td></tr>
                <tr><th>Model</th><td>${item.model ?? '-'}</td></tr>
                <tr><th>Spec</th><td>${item.spec ?? '-'}</td></tr>
                <tr><th>SN</th><td>${item.sn ?? '-'}</td></tr>
                <tr><th>Customer</th><td>${item.customer ?? '-'}</td></tr>
                <tr><th>Sales Month</th><td>${item.salesMonth ?? '-'}</td></tr>
                <tr><th>Destination</th><td>${item.destination ?? '-'}</td></tr>
                <tr><th>Del Date</th><td>${item.deldate ?? '-'}</td></tr>
                <tr><th>Contract Type</th><td>${item.contractType ?? '-'}</td></tr>
                <tr><th>Admin Fee</th><td>${item.adminFee ?? '-'}</td></tr>
                <tr><th>Amount Provision</th><td>${item.amountProvision ?? '-'}</td></tr>
                <tr><th>GA</th><td>${item.ga ?? '-'}</td></tr>
                <tr><th>Remark Support</th><td>${item.reemarkSupport ?? '-'}</td></tr>
            `;

                $('#invoiceDetailBody').html(html);
                $('#invoiceDetailModal').modal('show');
            },
            error: function (err) {
                console.error(err);
                alert('Error retrieving invoice detail.');
            }
        });
    });

}

function isNullOrEmpty(value) {
    return value === null || value === undefined || value === "undefined" || value === "";
}



$(document).on('click', '#btn-download-pdf', function () {
    const invoiceId = $(this).data('id');

    Swal.fire({
        title: 'Generating PDF...',
        text: 'Please wait',
        allowOutsideClick: false,
        showConfirmButton: false,
        didOpen: () => Swal.showLoading()
    });

    $.ajax({
        url: `/InvoiceReview/DownloadPdf?id=${invoiceId}`,
        method: 'GET',
        xhrFields: {
            responseType: 'blob' // penting! agar dapat file biner
        },
        success: function (data, status, xhr) {
            Swal.close();

            // ambil nama file dari header Content-Disposition (jika ada)
            let filename = "Invoice.pdf";
            const disposition = xhr.getResponseHeader('Content-Disposition');
            if (disposition && disposition.indexOf('filename=') !== -1) {
                filename = disposition.split('filename=')[1].trim().replace(/"/g, '');
            }

            // buat blob dan URL download
            const blob = new Blob([data], { type: 'application/pdf' });
            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            // redirect setelah download
            setTimeout(() => {
                window.location.href = '/InvoiceReview';
            }, 1000);
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Download Failed',
                text: 'Failed to generate or download PDF.'
            });
        }
    });
});

