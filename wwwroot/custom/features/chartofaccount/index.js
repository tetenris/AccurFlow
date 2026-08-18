let dataTable;
let isEditMode = false;

$(document).ready(function () {
    initializeDataTable();
    initializeEventHandlers();
});

function initializeDataTable() {
    dataTable = $('#chartofaccount_datatable').DataTable({
        processing: true,
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
            if ($('#chartofaccount_datatable_length').find('span.mr-2').length === 0) {
                $('#chartofaccount_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            // Add search icon to filter
            $('#chartofaccount_datatable_filter').css('position', 'relative');
            $('#chartofaccount_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');

            // Style search box
            $('#chartofaccount_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            // Style show dropdown
            $('#chartofaccount_datatable_length select').css({
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
            url: '/ChartOfAccount/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const accountType = $('#filter-account-type').val();
                const isActive = $('#filter-status').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    AccountType: accountType === "" ? null : accountType,
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
            {
                data: 'accountCode',
                render: function (data, type, row) {
                    return data;
                }
            },
            {
                data: 'accountName',
                render: function (data, type, row) {
                    if (row.isHeader) {
                        return '<strong>' + data + '</strong>';
                    }
                    return data;
                }
            },
            {
                data: 'accountType',
                render: function (data) {
                    const badges = {
                        'Asset': 'badge-light-primary',
                        'Liability': 'badge-light-danger',
                        'Equity': 'badge-light-warning',
                        'Revenue': 'badge-light-success',
                        'Expense': 'badge-light-info',
                        'Other Income': 'badge-light-success',
                        'Other Expense': 'badge-light-danger'
                    };
                    return `<span class="badge ${badges[data] || 'badge-light-secondary'}">${data}</span>`;
                }
            },
            {
                data: 'parentAccountName',
                render: function (data) {
                    return data || '-';
                }
            },
            {
                data: 'isActive',
                render: function (data) {
                    return data
                        ? '<span class="badge badge-light-success">Active</span>'
                        : '<span class="badge badge-light-warning">Inactive</span>';
                }
            },
            {
                data: "accountId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    let buttons = `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-primary btn-detail" data-id="${data}" title="View Detail">
                                <i class='fa fa-eye'></i>
                            </button>
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}" title="Edit">
                                <i class='fa fa-pencil'></i>
                            </button>`;
                    
                    // Only show delete button if account is inactive
                    if (!row.isActive) {
                        buttons += `
                            <button class="btn btn-sm btn-icon btn-light-danger btn-delete" data-id="${data}" title="Delete">
                                <i class='fa fa-trash-alt'></i>
                            </button>`;
                    }
                    
                    buttons += `
                            <button class="btn btn-sm btn-icon btn-light-warning btn-toggle" data-id="${data}" title="Toggle Status">
                                <i class='fa fa-toggle-${row.isActive ? 'on' : 'off'}'></i>
                            </button>
                        </div>
                    `;
                    return buttons;
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
        id: 'btn-add-account',
        html: '<i class="fa fa-add"></i> Add Account'
    });
    $('#toolbar-section-button').append(btnAdd);

    // Export button to toolbar
    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-account',
        html: '<i class="fa fa-file-excel"></i> Export Excel'
    });
    $('#toolbar-section-button').append(btnExport);

    // Download Template button to toolbar
    var btnTemplate = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-download-template',
        html: '<i class="fa fa-file-arrow-down"></i> Download Template'
    });
    $('#toolbar-section-button').append(btnTemplate);

    // Import button to toolbar
    var btnImport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-import-account',
        html: '<i class="fa fa-file-import"></i> Import'
    });
    $('#toolbar-section-button').append(btnImport);

    // Add account button (using event delegation)
    $(document).on('click', '#btn-add-account', function () {
        openModal(false);
    });

    // Export button click
    $(document).on('click', '#btn-export-account', function () {
        exportToExcel();
    });

    // Download Template button click
    $(document).on('click', '#btn-download-template', function () {
        downloadTemplate();
    });

    // Import button click
    $(document).on('click', '#btn-import-account', function () {
        $('#import-file').val('');
        $('#import-result').addClass('d-none');
        $('#import-errors').addClass('d-none');
        $('#import-error-list').empty();
        $('#modal-import-account').modal('show');
    });

    // Import submit
    $(document).on('click', '#btn-import-submit', function () {
        importAccounts();
    });

    // Account type change - load parent accounts and generate code
    $('#account-type').on('change', function () {
        const accountType = $(this).val();
        if (accountType) {
            loadParentAccounts(accountType);
            if (!isEditMode) {
                generateAccountCode();
            }
        }
    });

    // Parent account change - generate code
    $('#parent-account').on('change', function () {
        if (!isEditMode) {
            generateAccountCode();
        }
    });

    // Account code validation
    $('#account-code').on('blur', function () {
        validateAccountCode();
    });

    // Save button
    $('#btn-save-account').on('click', function () {
        saveAccount();
    });
}

function openModal(editMode, accountId = null) {
    isEditMode = editMode;
    $('#form-account')[0].reset();
    $('#account-id').val('');
    $('#parent-account').empty().append('<option value="">None (Root Account)</option>');

    if (editMode && accountId) {
        $('#modal-account-title').text('Edit Account');
        loadAccountData(accountId);
    } else {
        $('#modal-account-title').text('Add Account');
        $('#is-active').prop('checked', true);
        $('#is-header').prop('checked', false);
        $('#opening-balance').val(0);
        $('#currency').val('IDR');
    }

    $('#modal-account').modal('show');
}

function loadAccountData(accountId) {
    $.ajax({
        url: '/ChartOfAccount/GetById',
        type: 'GET',
        data: { id: accountId },
        success: function (data) {
            $('#account-id').val(data.accountId);
            $('#account-code').val(data.accountCode);
            $('#account-name').val(data.accountName);
            $('#account-type').val(data.accountType);
            $('#account-description').val(data.description);
            $('#opening-balance').val(data.openingBalance);
            $('#currency').val(data.currency);
            $('#is-header').prop('checked', data.isHeader);
            $('#is-active').prop('checked', data.isActive);

            // Load parent accounts and set selected
            loadParentAccounts(data.accountType, data.parentAccountId);
        },
        error: function () {
            Swal.fire('Error', 'Failed to load account data', 'error');
        }
    });
}

function loadParentAccounts(accountType, selectedId = null) {
    $.ajax({
        url: '/ChartOfAccount/GetParentAccounts',
        type: 'GET',
        data: { accountType: accountType },
        success: function (data) {
            const $select = $('#parent-account');
            $select.empty().append('<option value="">None (Root Account)</option>');

            data.forEach(function (account) {
                const indent = '&nbsp;&nbsp;&nbsp;'.repeat(account.level);
                $select.append(`<option value="${account.accountId}">${indent}${account.accountCode} - ${account.accountName}</option>`);
            });

            if (selectedId) {
                $select.val(selectedId);
            }
        }
    });
}

function generateAccountCode() {
    const accountType = $('#account-type').val();
    const parentId = $('#parent-account').val();

    if (!accountType) return;

    $.ajax({
        url: '/ChartOfAccount/GenerateCode',
        type: 'GET',
        data: {
            accountType: accountType,
            parentId: parentId || null
        },
        success: function (data) {
            $('#account-code').val(data.code);
        }
    });
}

function validateAccountCode() {
    const code = $('#account-code').val();
    const accountId = $('#account-id').val();

    if (!code) return;

    $.ajax({
        url: '/ChartOfAccount/ValidateCode',
        type: 'GET',
        data: {
            code: code,
            excludeId: accountId || null
        },
        success: function (data) {
            if (!data.isUnique) {
                Swal.fire('Warning', 'Account code already exists', 'warning');
                $('#account-code').addClass('is-invalid');
            } else {
                $('#account-code').removeClass('is-invalid');
            }
        }
    });
}

function saveAccount() {
    if (!$('#form-account')[0].checkValidity()) {
        $('#form-account')[0].reportValidity();
        return;
    }

    const accountId = $('#account-id').val();
    const url = accountId ? '/ChartOfAccount/Edit' : '/ChartOfAccount/Create';

    const data = {
        accountCode: $('#account-code').val(),
        accountName: $('#account-name').val(),
        accountType: $('#account-type').val(),
        description: $('#account-description').val(),
        parentAccountId: $('#parent-account').val() || null,
        isHeader: $('#is-header').is(':checked'),
        isActive: $('#is-active').is(':checked'),
        openingBalance: parseFloat($('#opening-balance').val()) || 0,
        currency: $('#currency').val()
    };

    if (accountId) {
        data.accountId = accountId;
    }

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-account').modal('hide');
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

// Edit Account
$('#chartofaccount_datatable').on('click', '.btn-edit', function () {
    const accountId = $(this).data('id');
    openModal(true, accountId);
});

// Delete Account
$('#chartofaccount_datatable').on('click', '.btn-delete', function () {
    const accountId = $(this).data('id');
    
    Swal.fire({
        title: 'Delete selected data?',
        text: "Are you sure you want to delete this account? This action cannot be undone.",
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
                url: '/ChartOfAccount/Delete',
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(accountId),
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
$('#chartofaccount_datatable').on('click', '.btn-toggle', function () {
    const accountId = $(this).data('id');
    
    $.ajax({
        url: '/ChartOfAccount/ToggleStatus',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(accountId),
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
    const accountType = $('#filter-account-type').val();
    const isActive = $('#filter-status').val();
    
    // Build query string
    let queryParams = [];
    if (accountType) queryParams.push(`accountType=${encodeURIComponent(accountType)}`);
    if (isActive !== '') queryParams.push(`isActive=${isActive}`);
    
    const queryString = queryParams.length > 0 ? '?' + queryParams.join('&') : '';
    
    // Trigger download
    window.location.href = '/ChartOfAccount/ExportExcel' + queryString;
}

// Download Template
function downloadTemplate() {
    window.location.href = '/ChartOfAccount/DownloadTemplate';
}

// Import Accounts
function importAccounts() {
    const fileInput = $('#import-file')[0];
    if (!fileInput.files || fileInput.files.length === 0) {
        Swal.fire('Warning', 'Please select an Excel file first', 'warning');
        return;
    }

    const file = fileInput.files[0];
    const formData = new FormData();
    formData.append('file', file);

    $('#btn-import-submit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Importing...');

    $.ajax({
        url: '/ChartOfAccount/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (data) {
            $('#btn-import-submit').prop('disabled', false).html('Import');

            Swal.fire('Success', data.message, 'success');
            if (data.skippedCount > 0) {
                showImportErrors(data);
            }
            $('#modal-import-account').modal('hide');
            dataTable.ajax.reload();
        },
        error: function (xhr) {
            $('#btn-import-submit').prop('disabled', false).html('Import');

            let message = xhr.responseJSON?.message || 'An error occurred during import';
            Swal.fire('Error', message, 'error');
        }
    });
}

function showImportErrors(data) {
    $('#import-result').removeClass('d-none');
    $('#import-errors').removeClass('d-none');
    $('#import-error-list').empty();

    $('#import-result-alert')
        .removeClass('alert-success alert-danger alert-warning')
        .addClass('alert-warning')
        .text(data.message);

    (data.errors || []).forEach(function (err) {
        $('#import-error-list').append(`<li class="text-danger">${err}</li>`);
    });
}

// View Detail
$('#chartofaccount_datatable').on('click', '.btn-detail', function () {
    const accountId = $(this).data('id');
    
    $.ajax({
        url: '/ChartOfAccount/GetById',
        type: 'GET',
        data: { id: accountId },
        success: function (data) {
            $('#detail-account-code').text(data.accountCode);
            $('#detail-account-name').text(data.accountName);
            
            const typeBadges = {
                'Asset': 'badge-light-primary',
                'Liability': 'badge-light-danger',
                'Equity': 'badge-light-warning',
                'Revenue': 'badge-light-success',
                'Expense': 'badge-light-info',
                'Other Income': 'badge-light-success',
                'Other Expense': 'badge-light-danger'
            };
            $('#detail-account-type').html(`<span class="badge ${typeBadges[data.accountType]}">${data.accountType}</span>`);
            
            $('#detail-parent-account').text(data.parentAccountName || '-');
            $('#detail-normal-balance').text(data.normalBalance);
            $('#detail-opening-balance').text(data.openingBalance.toLocaleString('en-US', { minimumFractionDigits: 2 }));
            $('#detail-currency').text(data.currency || 'IDR');
            $('#detail-is-header').html(data.isHeader ? '<span class="badge badge-light-info">Yes</span>' : '<span class="badge badge-light-secondary">No</span>');
            $('#detail-status').html(data.isActive ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>');
            $('#detail-level').text(data.level);
            $('#detail-description').text(data.description || '-');
            $('#detail-created-by').text(data.createdBy || '-');
            $('#detail-created-at').text(data.createdAt ? new Date(data.createdAt).toLocaleString('en-GB') : '-');
            $('#detail-updated-by').text(data.updatedBy || '-');
            $('#detail-updated-at').text(data.updatedAt ? new Date(data.updatedAt).toLocaleString('en-GB') : '-');
            
            $('#modal-account-detail').modal('show');
        },
        error: function () {
            Swal.fire('Error', 'Failed to load account detail', 'error');
        }
    });
});
