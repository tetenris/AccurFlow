let dataTable;
let isEditMode = false;
let journalLines = [];

$(document).ready(function () {
    initializeDataTable();
    initializeEventHandlers();
    loadAccountDropdown();
});

function initializeDataTable() {
    dataTable = $('#journalentry_datatable').DataTable({
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
            if ($('#journalentry_datatable_length').find('span.mr-2').length === 0) {
                $('#journalentry_datatable_length').prepend('<span class="mr-2" style="margin-right: 10px;">Show</span>');
            }
            $('#journalentry_datatable_filter').css('position', 'relative');
            $('#journalentry_datatable_filter').prepend('<i class="fa fa-search" style="position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #a1a5b7; pointer-events: none;"></i>');
            $('#journalentry_datatable_filter input').css({
                'width': '400px',
                'display': 'inline-block',
                'border': '1px solid #d1d5db',
                'border-radius': '0.375rem',
                'padding': '0.5rem 0.75rem 0.5rem 2.5rem',
                'height': '42px',
                'font-size': '1rem'
            });
            $('#journalentry_datatable_length select').css({
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
            url: '/JournalEntry/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const dateFrom = $('#filter-date-from').val();
                const dateTo = $('#filter-date-to').val();
                const status = $('#filter-status').val();
                const accountId = $('#filter-account').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    DateFrom: dateFrom || null,
                    DateTo: dateTo || null,
                    Status: status || null,
                    AccountId: accountId || null
                });
            }
        },
        order: [[2, 'desc']],
        columns: [
            {
                data: null,
                orderable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: 'journalNumber' },
            {
                data: 'journalDate',
                render: function (data) {
                    return new Date(data).toLocaleDateString('en-GB');
                }
            },
            { data: 'description' },
            {
                data: 'totalDebit',
                render: function (data) {
                    return data.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }
            },
            {
                data: 'status',
                render: function (data) {
                    const badges = {
                        'Draft': 'badge-light-warning',
                        'Posted': 'badge-light-success',
                        'Reversed': 'badge-light-danger'
                    };
                    return `<span class="badge ${badges[data] || 'badge-light-secondary'}">${data}</span>`;
                }
            },
            {
                data: "journalId",
                orderable: false,
                searchable: false,
                className: 'action-cell text-center',
                render: function (data, type, row) {
                    let buttons = `
                        <div class="d-flex gap-2 justify-content-center">
                            <button class="btn btn-sm btn-icon btn-light-primary btn-detail" data-id="${data}" title="View Detail">
                                <i class='fa fa-eye'></i>
                            </button>`;
                    
                    // Only allow edit for Draft status
                    if (row.canEdit) {
                        buttons += `
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}" title="Edit">
                                <i class='fa fa-pencil'></i>
                            </button>`;
                    }
                    
                    // No delete button - transactions should never be deleted
                    // Use Reverse instead for posted journals
                    
                    buttons += `</div>`;
                    return buttons;
                }
            }
        ]
    });
}

function initializeEventHandlers() {
    $('#btn-apply-filter').on('click', function () {
        dataTable.ajax.reload();
    });

    var btnAdd = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-add-journal',
        html: '<i class="fa fa-add"></i> Add Journal Entry'
    });
    $('#toolbar-section-button').append(btnAdd);

    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-journal',
        html: '<i class="fa fa-file-excel"></i> Export Excel'
    });
    $('#toolbar-section-button').append(btnExport);

    $(document).on('click', '#btn-add-journal', function () {
        openModal(false);
    });

    $(document).on('click', '#btn-export-journal', function () {
        exportToExcel();
    });

    $('#journal-date').on('change', function () {
        if (!isEditMode) {
            generateJournalNumber();
        }
    });

    $('#btn-add-line').on('click', function () {
        addJournalLine();
    });

    $('#btn-save-journal').on('click', function () {
        saveJournal();
    });

    $(document).on('click', '.btn-remove-line', function () {
        $(this).closest('tr').remove();
        calculateTotals();
    });

    $(document).on('input', '.line-debit, .line-credit', function () {
        calculateTotals();
    });
}

function loadAccountDropdown() {
    $.ajax({
        url: '/JournalEntry/GetAccountDropdown',
        type: 'GET',
        success: function (data) {
            console.log('Account dropdown data:', data);
            const $filterSelect = $('#filter-account');
            $filterSelect.empty().append('<option value="">All Accounts</option>');
            
            if (data && data.length > 0) {
                $.each(data, function (index, item) {
                    $filterSelect.append($('<option>', {
                        value: item.value,
                        text: item.text
                    }));
                });
            } else {
                console.warn('No detail accounts found. Please ensure you have non-header active accounts in Chart of Accounts.');
            }
        },
        error: function(xhr, status, error) {
            console.error('Failed to load accounts:', error);
            Swal.fire('Error', 'Failed to load account list. Please check Chart of Accounts.', 'error');
        }
    });
}

function openModal(editMode, journalId = null) {
    isEditMode = editMode;
    $('#form-journal')[0].reset();
    $('#journal-id').val('');
    $('#journal-lines-body').empty();
    journalLines = [];

    if (editMode && journalId) {
        $('#modal-journal-title').text('Edit Journal Entry');
        loadJournalData(journalId);
    } else {
        $('#modal-journal-title').text('Add Journal Entry');
        $('#journal-date').val(new Date().toISOString().split('T')[0]);
        generateJournalNumber();
        addJournalLine();
        addJournalLine();
    }

    $('#modal-journal').modal('show');
}

function loadJournalData(journalId) {
    $.ajax({
        url: '/JournalEntry/GetById',
        type: 'GET',
        data: { id: journalId },
        success: function (data) {
            $('#journal-id').val(data.journalId);
            $('#journal-number').val(data.journalNumber);
            $('#journal-date').val(data.journalDate.split('T')[0]);
            $('#journal-description').val(data.description);

            journalLines = data.journalLines;
            renderJournalLines();
            calculateTotals();
        },
        error: function () {
            Swal.fire('Error', 'Failed to load journal data', 'error');
        }
    });
}

function generateJournalNumber() {
    const journalDate = $('#journal-date').val();
    if (!journalDate) return;

    $.ajax({
        url: '/JournalEntry/GenerateNumber',
        type: 'GET',
        data: { journalDate: journalDate },
        success: function (data) {
            $('#journal-number').val(data.number);
        }
    });
}

function addJournalLine() {
    $.ajax({
        url: '/JournalEntry/GetAccountDropdown',
        type: 'GET',
        success: function (accounts) {
            console.log('Accounts for journal line:', accounts);
            
            if (!accounts || accounts.length === 0) {
                Swal.fire({
                    title: 'No Accounts Available',
                    html: 'No detail accounts found. Please:<br/>1. Go to <b>Database Seeding</b> menu and seed Chart of Accounts<br/>2. Or ensure you have non-header active accounts in Chart of Accounts',
                    icon: 'warning'
                });
                return;
            }
            
            const lineIndex = $('#journal-lines-body tr').length;
            let accountOptions = '<option value="">Select Account</option>';
            accounts.forEach(function (account) {
                accountOptions += `<option value="${account.value}">${account.text}</option>`;
            });

            const row = `
                <tr>
                    <td>
                        <select class="form-select line-account" required>
                            ${accountOptions}
                        </select>
                    </td>
                    <td>
                        <input type="text" class="form-control line-description" maxlength="500" />
                    </td>
                    <td>
                        <input type="number" class="form-control line-debit" step="0.01" min="0" value="0" />
                    </td>
                    <td>
                        <input type="number" class="form-control line-credit" step="0.01" min="0" value="0" />
                    </td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-icon btn-light-danger btn-remove-line">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
            $('#journal-lines-body').append(row);
        },
        error: function(xhr, status, error) {
            console.error('Failed to load accounts for journal line:', error);
            Swal.fire('Error', 'Failed to load account list. Please check Chart of Accounts.', 'error');
        }
    });
}

function renderJournalLines() {
    $('#journal-lines-body').empty();
    
    $.ajax({
        url: '/JournalEntry/GetAccountDropdown',
        type: 'GET',
        success: function (accounts) {
            journalLines.forEach(function (line) {
                let accountOptions = '<option value="">Select Account</option>';
                accounts.forEach(function (account) {
                    const selected = account.value === line.accountId ? 'selected' : '';
                    accountOptions += `<option value="${account.value}" ${selected}>${account.text}</option>`;
                });

                const row = `
                    <tr>
                        <td>
                            <select class="form-select line-account" required>
                                ${accountOptions}
                            </select>
                        </td>
                        <td>
                            <input type="text" class="form-control line-description" value="${line.description || ''}" maxlength="500" />
                        </td>
                        <td>
                            <input type="number" class="form-control line-debit" step="0.01" min="0" value="${line.debitAmount}" />
                        </td>
                        <td>
                            <input type="number" class="form-control line-credit" step="0.01" min="0" value="${line.creditAmount}" />
                        </td>
                        <td class="text-center">
                            <button type="button" class="btn btn-sm btn-icon btn-light-danger btn-remove-line">
                                <i class="fa fa-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
                $('#journal-lines-body').append(row);
            });
        }
    });
}

function calculateTotals() {
    let totalDebit = 0;
    let totalCredit = 0;

    $('#journal-lines-body tr').each(function () {
        const debit = parseFloat($(this).find('.line-debit').val()) || 0;
        const credit = parseFloat($(this).find('.line-credit').val()) || 0;
        totalDebit += debit;
        totalCredit += credit;
    });

    $('#total-debit').text(totalDebit.toFixed(2));
    $('#total-credit').text(totalCredit.toFixed(2));

    const difference = Math.abs(totalDebit - totalCredit);
    if (difference < 0.01) {
        $('#balance-indicator').html('<span class="badge badge-light-success"><i class="fa fa-check"></i> Balanced</span>');
    } else {
        $('#balance-indicator').html(`<span class="badge badge-light-danger"><i class="fa fa-times"></i> Not Balanced (Difference: ${difference.toFixed(2)})</span>`);
    }
}

function saveJournal() {
    if (!$('#form-journal')[0].checkValidity()) {
        $('#form-journal')[0].reportValidity();
        return;
    }

    const lines = [];
    let isValid = true;

    $('#journal-lines-body tr').each(function () {
        const accountId = $(this).find('.line-account').val();
        const description = $(this).find('.line-description').val();
        const debit = parseFloat($(this).find('.line-debit').val()) || 0;
        const credit = parseFloat($(this).find('.line-credit').val()) || 0;

        if (!accountId) {
            Swal.fire('Error', 'Please select account for all lines', 'error');
            isValid = false;
            return false;
        }

        if (debit === 0 && credit === 0) {
            Swal.fire('Error', 'Each line must have either debit or credit amount', 'error');
            isValid = false;
            return false;
        }

        if (debit > 0 && credit > 0) {
            Swal.fire('Error', 'A line cannot have both debit and credit amounts', 'error');
            isValid = false;
            return false;
        }

        lines.push({
            accountId: accountId,
            description: description,
            debitAmount: debit,
            creditAmount: credit
        });
    });

    if (!isValid) return;

    if (lines.length < 2) {
        Swal.fire('Error', 'At least 2 journal lines are required', 'error');
        return;
    }

    const totalDebit = lines.reduce((sum, line) => sum + line.debitAmount, 0);
    const totalCredit = lines.reduce((sum, line) => sum + line.creditAmount, 0);

    if (Math.abs(totalDebit - totalCredit) >= 0.01) {
        Swal.fire('Error', 'Journal is not balanced', 'error');
        return;
    }

    const journalId = $('#journal-id').val();
    const url = journalId ? '/JournalEntry/Edit' : '/JournalEntry/Create';

    const data = {
        journalDate: $('#journal-date').val(),
        description: $('#journal-description').val(),
        journalLines: lines
    };

    if (journalId) {
        data.journalId = journalId;
    }

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-journal').modal('hide');
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

$('#journalentry_datatable').on('click', '.btn-detail', function () {
    const journalId = $(this).data('id');
    viewDetail(journalId);
});

$('#journalentry_datatable').on('click', '.btn-edit', function () {
    const journalId = $(this).data('id');
    openModal(true, journalId);
});

$('#journalentry_datatable').on('click', '.btn-delete', function () {
    const journalId = $(this).data('id');
    
    Swal.fire({
        title: 'Delete selected data?',
        text: "Are you sure you want to delete this journal entry?",
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
                url: '/JournalEntry/Delete',
                type: 'DELETE',
                contentType: 'application/json',
                data: JSON.stringify(journalId),
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

function viewDetail(journalId) {
    $.ajax({
        url: '/JournalEntry/GetById',
        type: 'GET',
        data: { id: journalId },
        success: function (data) {
            $('#detail-number').text(data.journalNumber);
            $('#detail-date').text(new Date(data.journalDate).toLocaleDateString('en-GB'));
            $('#detail-description').text(data.description);
            
            const statusBadges = {
                'Draft': 'badge-light-warning',
                'Posted': 'badge-light-success',
                'Reversed': 'badge-light-danger'
            };
            $('#detail-status').html(`<span class="badge ${statusBadges[data.status]}">${data.status}</span>`);
            
            $('#detail-created-by').text(data.createdBy + ' at ' + new Date(data.createdAt).toLocaleString('en-GB'));
            $('#detail-posted-by').text(data.postedBy ? data.postedBy + ' at ' + new Date(data.postedDate).toLocaleString('en-GB') : '-');

            $('#detail-lines-body').empty();
            data.journalLines.forEach(function (line) {
                const row = `
                    <tr>
                        <td>${line.lineNumber}</td>
                        <td>${line.accountCode} - ${line.accountName}</td>
                        <td>${line.description || '-'}</td>
                        <td>${line.debitAmount.toFixed(2)}</td>
                        <td>${line.creditAmount.toFixed(2)}</td>
                    </tr>
                `;
                $('#detail-lines-body').append(row);
            });

            $('#detail-total-debit').text(data.totalDebit.toFixed(2));
            $('#detail-total-credit').text(data.totalCredit.toFixed(2));

            $('#btn-post-journal').hide();
            $('#btn-reverse-journal').hide();

            if (data.canPost) {
                $('#btn-post-journal').show().data('id', journalId);
            }

            if (data.canReverse) {
                $('#btn-reverse-journal').show().data('id', journalId);
            }

            $('#modal-detail').modal('show');
        },
        error: function () {
            Swal.fire('Error', 'Failed to load journal detail', 'error');
        }
    });
}

$('#btn-post-journal').on('click', function () {
    const journalId = $(this).data('id');
    
    Swal.fire({
        title: 'Post Journal Entry?',
        text: "Once posted, this journal cannot be edited or deleted.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Post',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-success',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/JournalEntry/Post',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    journalId: journalId,
                    postedDate: new Date().toISOString()
                }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-detail').modal('hide');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    const message = xhr.responseJSON?.message || 'An error occurred';
                    Swal.fire('Error', message, 'error');
                }
            });
        }
    });
});

$('#btn-reverse-journal').on('click', function () {
    const journalId = $(this).data('id');
    
    Swal.fire({
        title: 'Reverse Journal Entry?',
        html: `
            <p>This will create a reversal journal entry.</p>
            <label>Reversal Date:</label>
            <input type="date" id="reversal-date" class="form-control" value="${new Date().toISOString().split('T')[0]}" />
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Reverse',
        cancelButtonText: 'Cancel',
        customClass: {
            confirmButton: 'btn btn-danger',
            cancelButton: 'btn btn-secondary'
        },
        buttonsStyling: false,
        preConfirm: () => {
            const reversalDate = document.getElementById('reversal-date').value;
            if (!reversalDate) {
                Swal.showValidationMessage('Please select reversal date');
            }
            return reversalDate;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/JournalEntry/Reverse',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    journalId: journalId,
                    reversalDate: result.value
                }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-detail').modal('hide');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    const message = xhr.responseJSON?.message || 'An error occurred';
                    Swal.fire('Error', message, 'error');
                }
            });
        }
    });
});

function exportToExcel() {
    const dateFrom = $('#filter-date-from').val();
    const dateTo = $('#filter-date-to').val();
    const status = $('#filter-status').val();
    const accountId = $('#filter-account').val();
    
    let queryParams = [];
    if (dateFrom) queryParams.push(`dateFrom=${dateFrom}`);
    if (dateTo) queryParams.push(`dateTo=${dateTo}`);
    if (status) queryParams.push(`status=${encodeURIComponent(status)}`);
    if (accountId) queryParams.push(`accountId=${accountId}`);
    
    const queryString = queryParams.length > 0 ? '?' + queryParams.join('&') : '';
    window.location.href = '/JournalEntry/ExportExcel' + queryString;
}
