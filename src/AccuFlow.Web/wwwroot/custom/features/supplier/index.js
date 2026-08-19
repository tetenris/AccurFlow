let dataTable;
let isEditMode = false;

$(document).ready(function () {
    initializeDataTable();
    initializeEventHandlers();
});

function initializeDataTable() {
    dataTable = $('#supplier_datatable').DataTable({
        processing: true,
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
            if ($('#supplier_datatable_length').find('span.mr-2').length === 0) {
                $('#supplier_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            $('#supplier_datatable_filter').css('position', 'relative');
            $('#supplier_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');
            $('#supplier_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            $('#supplier_datatable_length select').css({
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 2rem 0.5rem 0.75rem',
                'height': '42px',
                'font-size': '1rem'
            });
            $('.top').css({
                'display': 'flex',
                'align-items': 'center',
                'gap': '15px'
            });
        },
        ajax: {
            url: '/Supplier/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const supplierType = $('#filter-supplier-type').val();
                const isActive = $('#filter-status').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    SupplierType: supplierType === "" ? null : supplierType,
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
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: 'supplierCode' },
            { data: 'supplierName' },
            {
                data: 'supplierType',
                render: function (data) {
                    const badges = {
                        'Individual': 'badge-light-primary',
                        'Corporate': 'badge-light-success',
                        'Government': 'badge-light-info'
                    };
                    return `<span class="badge ${badges[data] || 'badge-light-secondary'}">${data}</span>`;
                }
            },
            { data: 'contactPerson', render: (data) => data || '-' },
            { data: 'phone', render: (data) => data || '-' },
            { data: 'email', render: (data) => data || '-' },
            {
                data: 'isActive',
                render: function (data) {
                    return data
                        ? '<span class="badge badge-light-success">Active</span>'
                        : '<span class="badge badge-light-warning">Inactive</span>';
                }
            },
            {
                data: "supplierId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-primary btn-detail" data-id="${data}" title="View Detail">
                                <i class='fa fa-eye'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}" title="Edit">
                                <i class='fa fa-pencil'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-danger btn-delete" data-id="${data}" title="Delete">
                                <i class='fa fa-trash-alt'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-warning btn-toggle" data-id="${data}" title="Toggle Status">
                                <i class='fa fa-toggle-${row.isActive ? 'on' : 'off'}'></i>
                            </button>
                        </div>
                    `;
                }
            }
        ]
    });
}

function initializeEventHandlers() {
    // Apply filter button
    $('#btn-apply-filter').on('click', function () {
        dataTable.ajax.reload();
    });

    // Add button to toolbar
    var btnAdd = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-add-supplier',
        html: '<i class="fa fa-add"></i> Add Supplier'
    });
    $('#toolbar-section-button').append(btnAdd);

    // Export button to toolbar
    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-supplier',
        html: '<i class="fa fa-file-excel"></i> Export Excel'
    });
    $('#toolbar-section-button').append(btnExport);

    // Add supplier button
    $(document).on('click', '#btn-add-supplier', function () {
        openModal(false);
    });

    // Export button click
    $(document).on('click', '#btn-export-supplier', function () {
        exportToExcel();
    });

    // Save button
    $('#btn-save-supplier').on('click', function () {
        saveSupplier();
    });
}

function openModal(editMode, supplierId = null) {
    isEditMode = editMode;
    $('#form-supplier')[0].reset();
    $('#supplier-id').val('');
    
    // Reset to first tab
    $('#basic-tab').tab('show');

    if (editMode && supplierId) {
        $('#modal-supplier-title').text('Edit Supplier');
        loadSupplierData(supplierId);
    } else {
        $('#modal-supplier-title').text('Add Supplier');
        $('#supplier-active').prop('checked', true);
        $('#credit-limit').val(0);
        $('#payment-terms').val(30);
        // Generate supplier code
        generateSupplierCode();
    }

    $('#modal-supplier').modal('show');
}

function loadSupplierData(supplierId) {
    $.ajax({
        url: '/Supplier/GetById',
        type: 'GET',
        data: { id: supplierId },
        success: function (data) {
            $('#supplier-id').val(data.supplierId);
            $('#supplier-code').val(data.supplierCode);
            $('#supplier-name').val(data.supplierName);
            $('#supplier-type').val(data.supplierType);
            $('#contact-person').val(data.contactPerson);
            $('#phone').val(data.phone);
            $('#email').val(data.email);
            $('#website').val(data.website);
            $('#address').val(data.address);
            $('#city').val(data.city);
            $('#state').val(data.state);
            $('#postal-code').val(data.postalCode);
            $('#country').val(data.country);
            $('#credit-limit').val(data.creditLimit);
            $('#payment-terms').val(data.paymentTerms);
            $('#tax-id').val(data.taxId);
            $('#notes').val(data.notes);
            $('#supplier-active').prop('checked', data.isActive);
        },
        error: function () {
            Swal.fire('Error', 'Failed to load supplier data', 'error');
        }
    });
}

function generateSupplierCode() {
    $.ajax({
        url: '/Supplier/GenerateCode',
        type: 'GET',
        success: function (data) {
            $('#supplier-code').val(data.code);
        }
    });
}

function saveSupplier() {
    if (!$('#form-supplier')[0].checkValidity()) {
        $('#form-supplier')[0].reportValidity();
        return;
    }

    const supplierId = $('#supplier-id').val();
    const url = supplierId ? '/Supplier/Edit' : '/Supplier/Create';

    const data = {
        supplierCode: $('#supplier-code').val(),
        supplierName: $('#supplier-name').val(),
        supplierType: $('#supplier-type').val(),
        contactPerson: $('#contact-person').val() || null,
        phone: $('#phone').val() || null,
        email: $('#email').val() || null,
        website: $('#website').val() || null,
        address: $('#address').val() || null,
        city: $('#city').val() || null,
        state: $('#state').val() || null,
        postalCode: $('#postal-code').val() || null,
        country: $('#country').val() || null,
        creditLimit: parseFloat($('#credit-limit').val()) || 0,
        paymentTerms: parseInt($('#payment-terms').val()) || 30,
        taxId: $('#tax-id').val() || null,
        notes: $('#notes').val() || null,
        isActive: $('#supplier-active').is(':checked')
    };

    if (supplierId) {
        data.supplierId = supplierId;
    }

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-supplier').modal('hide');
                dataTable.ajax.reload();
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || 'An error occurred';
            Swal.fire('Error', message, 'error');
        }
    });
}

// View Detail Supplier
$('#supplier_datatable').on('click', '.btn-detail', function () {
    const supplierId = $(this).data('id');
    viewSupplierDetail(supplierId);
});

// Edit Supplier
$('#supplier_datatable').on('click', '.btn-edit', function () {
    const supplierId = $(this).data('id');
    openModal(true, supplierId);
});

// Delete Supplier
$('#supplier_datatable').on('click', '.btn-delete', function () {
    const supplierId = $(this).data('id');
    
    Swal.fire({
        title: 'Delete selected data?',
        text: "Are you sure you want to delete this supplier? This action cannot be undone.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-danger',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Supplier/Delete',
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(supplierId),
                success: function (response) {
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
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    const message = xhr.responseJSON?.message || 'An error occurred';
                    Swal.fire({
                        title: 'Error',
                        text: message,
                        icon: 'error',
                        confirmButtonText: 'OK',
                        customClass: {
                            confirmButton: 'btn btn-danger'
                        },
                        buttonsStyling: false
                    });
                }
            });
        }
    });
});

// Toggle Status
$('#supplier_datatable').on('click', '.btn-toggle', function () {
    const supplierId = $(this).data('id');
    
    $.ajax({
        url: '/Supplier/ToggleStatus',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(supplierId),
        success: function (response) {
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
                dataTable.ajax.reload();
            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || 'An error occurred';
            Swal.fire({
                title: 'Error',
                text: message,
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

// Export to Excel
function exportToExcel() {
    const supplierType = $('#filter-supplier-type').val();
    const isActive = $('#filter-status').val();
    
    let queryParams = [];
    if (supplierType) queryParams.push(`supplierType=${encodeURIComponent(supplierType)}`);
    if (isActive !== '') queryParams.push(`isActive=${isActive}`);
    
    const queryString = queryParams.length > 0 ? '?' + queryParams.join('&') : '';
    
    window.location.href = '/Supplier/ExportExcel' + queryString;
}

// View Supplier Detail
function viewSupplierDetail(supplierId) {
    $.ajax({
        url: '/Supplier/GetById',
        type: 'GET',
        data: { id: supplierId },
        success: function (data) {
            // Basic Info
            $('#detail-supplier-code').text(data.supplierCode);
            $('#detail-supplier-name').text(data.supplierName);
            
            const typeBadges = {
                'Individual': 'badge-light-primary',
                'Corporate': 'badge-light-success',
                'Government': 'badge-light-info'
            };
            $('#detail-supplier-type').html(`<span class="badge ${typeBadges[data.supplierType]}">${data.supplierType}</span>`);
            $('#detail-status').html(data.isActive 
                ? '<span class="badge badge-light-success">Active</span>' 
                : '<span class="badge badge-light-warning">Inactive</span>');
            
            // Contact Info
            $('#detail-contact-person').text(data.contactPerson || '-');
            $('#detail-phone').text(data.phone || '-');
            $('#detail-email').text(data.email || '-');
            $('#detail-website').text(data.website || '-');
            
            // Address Info
            $('#detail-address').text(data.address || '-');
            $('#detail-city').text(data.city || '-');
            $('#detail-state').text(data.state || '-');
            $('#detail-postal-code').text(data.postalCode || '-');
            $('#detail-country').text(data.country || '-');
            
            // Financial Info
            $('#detail-credit-limit').text(formatCurrency(data.creditLimit));
            $('#detail-payment-terms').text(data.paymentTerms + ' days');
            $('#detail-current-balance').text(formatCurrency(data.currentBalance));
            $('#detail-tax-id').text(data.taxId || '-');
            
            // Additional Info
            $('#detail-notes').text(data.notes || '-');
            
            // Audit Info
            $('#detail-created-by').text(data.createdBy || '-');
            $('#detail-created-at').text(data.createdAt ? new Date(data.createdAt).toLocaleString('en-GB') : '-');
            $('#detail-updated-by').text(data.updatedBy || '-');
            $('#detail-updated-at').text(data.updatedAt ? new Date(data.updatedAt).toLocaleString('en-GB') : '-');
            
            $('#modal-supplier-detail').modal('show');
        },
        error: function () {
            Swal.fire('Error', 'Failed to load supplier detail', 'error');
        }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', {
        style: 'currency',
        currency: 'IDR',
        minimumFractionDigits: 0
    }).format(amount);
}
