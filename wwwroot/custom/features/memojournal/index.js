let dataTable;
let isEditMode = false;
let journalLines = [];

$(document).ready(function () {
    initializeDataTable();
    initializeEventHandlers();
    loadAccountDropdown();
});

function initializeDataTable() {
    dataTable = $('#memojournal_datatable').DataTable({
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
        ajax: {
            url: '/MemoJournal/Datatable',
            type: 'POST',
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            data: function (d) {
                const dateFrom = $('#filter-date-from').val();
                const dateTo = $('#filter-date-to').val();
                const type = $('#filter-type').val();
                const status = $('#filter-status').val();
                return JSON.stringify({
                    Draw: d.draw,
                    Search: d.search.value || "",
                    OrderBy: (d.order && d.order.length > 0) ? d.columns[d.order[0].column].data : null,
                    OrderType: (d.order && d.order.length > 0) ? d.order[0].dir : null,
                    Page: (d.start / d.length) + 1,
                    Size: d.length,
                    DateFrom: dateFrom || null,
                    DateTo: dateTo || null,
                    JournalType: type || null,
                    Status: status || null
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
                    if (row.canEdit) {
                        buttons += `
                            <button class="btn btn-sm btn-icon btn-light-success btn-edit" data-id="${data}" title="Edit">
                                <i class='fa fa-pencil'></i>
                            </button>`;
                    }
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
        id: 'btn-add-memo',
        html: '<i class="fa fa-add"></i> Add Journal'
    });
    $('#toolbar-section-button').append(btnAdd);

    $(document).on('click', '#btn-add-memo', function () {
        openModal(false);
    });

    $('#memo-date').on('change', function () {
        if (!isEditMode) generateJournalNumber();
    });

    $('#memo-type').on('change', function () {
        if (!isEditMode) generateJournalNumber();
    });

    $('#btn-add-line').on('click', function () {
        addJournalLine();
    });

    $('#btn-save-memo').on('click', function () {
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
    $.get('/MemoJournal/GetAccountDropdown', function (data) { });
}

function openModal(editMode, journalId = null) {
    isEditMode = editMode;
    $('#form-memo')[0].reset();
    $('#memo-id').val('');
    $('#memo-lines-body').empty();
    journalLines = [];

    if (editMode && journalId) {
        $('#modal-memo-title').text('Edit Journal');
        loadJournalData(journalId);
    } else {
        $('#modal-memo-title').text('Add Journal');
        $('#memo-type').val('Memo');
        $('#memo-date').val(new Date().toISOString().split('T')[0]);
        generateJournalNumber();
        addJournalLine();
        addJournalLine();
    }

    $('#modal-memo').modal('show');
}

function loadJournalData(journalId) {
    $.ajax({
        url: '/MemoJournal/GetById',
        type: 'GET',
        data: { id: journalId },
        success: function (data) {
            $('#memo-id').val(data.journalId);
            $('#memo-number').val(data.journalNumber);
            $('#memo-date').val(data.journalDate.split('T')[0]);
            $('#memo-type').val(data.journalType).prop('disabled', true);
            $('#memo-description').val(data.description);

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
    const journalDate = $('#memo-date').val();
    const journalType = $('#memo-type').val() || 'Memo';
    if (!journalDate) return;

    $.get('/MemoJournal/GenerateNumber', { journalDate: journalDate, journalType: journalType }, function (data) {
        $('#memo-number').val(data.number);
    });
}

function addJournalLine() {
    $.get('/MemoJournal/GetAccountDropdown', function (accounts) {
        if (!accounts || accounts.length === 0) {
            Swal.fire({ title: 'No Accounts Available', text: 'No detail accounts found. Please seed Chart of Accounts first.', icon: 'warning' });
            return;
        }
        let accountOptions = '<option value="">Select Account</option>';
        accounts.forEach(function (account) {
            accountOptions += `<option value="${account.value}">${account.text}</option>`;
        });

        const row = `
            <tr>
                <td><select class="form-select line-account" required>${accountOptions}</select></td>
                <td><input type="text" class="form-control line-description" maxlength="500" /></td>
                <td><input type="number" class="form-control line-debit" step="0.01" min="0" value="0" /></td>
                <td><input type="number" class="form-control line-credit" step="0.01" min="0" value="0" /></td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-icon btn-light-danger btn-remove-line">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>`;
        $('#memo-lines-body').append(row);
    });
}

function renderJournalLines() {
    $('#memo-lines-body').empty();
    $.get('/MemoJournal/GetAccountDropdown', function (accounts) {
        journalLines.forEach(function (line) {
            let accountOptions = '<option value="">Select Account</option>';
            accounts.forEach(function (account) {
                const selected = account.value === line.accountId ? 'selected' : '';
                accountOptions += `<option value="${account.value}" ${selected}>${account.text}</option>`;
            });

            const row = `
                <tr>
                    <td><select class="form-select line-account" required>${accountOptions}</select></td>
                    <td><input type="text" class="form-control line-description" value="${line.description || ''}" maxlength="500" /></td>
                    <td><input type="number" class="form-control line-debit" step="0.01" min="0" value="${line.debitAmount}" /></td>
                    <td><input type="number" class="form-control line-credit" step="0.01" min="0" value="${line.creditAmount}" /></td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-icon btn-light-danger btn-remove-line">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>`;
            $('#memo-lines-body').append(row);
        });
    });
}

function calculateTotals() {
    let totalDebit = 0;
    let totalCredit = 0;

    $('#memo-lines-body tr').each(function () {
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
    if (!$('#form-memo')[0].checkValidity()) {
        $('#form-memo')[0].reportValidity();
        return;
    }

    const lines = [];
    let isValid = true;

    $('#memo-lines-body tr').each(function () {
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

        lines.push({ accountId: accountId, description: description, debitAmount: debit, creditAmount: credit });
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

    const journalId = $('#memo-id').val();
    const url = journalId ? '/MemoJournal/Edit' : '/MemoJournal/Create';

    const data = {
        journalDate: $('#memo-date').val(),
        description: $('#memo-description').val(),
        journalType: $('#memo-type').val() || 'Memo',
        journalLines: lines
    };
    if (journalId) data.journalId = journalId;

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                Swal.fire('Success', response.message, 'success');
                $('#modal-memo').modal('hide');
                dataTable.ajax.reload();
            } else {
                Swal.fire('Error', response.message, 'error');
            }
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
        }
    });
}

$('#memojournal_datatable').on('click', '.btn-detail', function () {
    viewDetail($(this).data('id'));
});

$('#memojournal_datatable').on('click', '.btn-edit', function () {
    openModal(true, $(this).data('id'));
});

function viewDetail(journalId) {
    $.ajax({
        url: '/MemoJournal/GetById',
        type: 'GET',
        data: { id: journalId },
        success: function (data) {
            $('#detail-number').text(data.journalNumber);
            $('#detail-type').text(data.journalType);
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
                $('#detail-lines-body').append(`
                    <tr>
                        <td>${line.lineNumber}</td>
                        <td>${line.accountCode} - ${line.accountName}</td>
                        <td>${line.description || '-'}</td>
                        <td>${line.debitAmount.toFixed(2)}</td>
                        <td>${line.creditAmount.toFixed(2)}</td>
                    </tr>`);
            });
            $('#detail-total-debit').text(data.totalDebit.toFixed(2));
            $('#detail-total-credit').text(data.totalCredit.toFixed(2));

            $('#btn-post-memo').hide().data('id', journalId);
            $('#btn-reverse-memo').hide().data('id', journalId);
            if (data.canPost) $('#btn-post-memo').show();
            if (data.canReverse) $('#btn-reverse-memo').show();

            $('#modal-detail').modal('show');
        },
        error: function () {
            Swal.fire('Error', 'Failed to load journal detail', 'error');
        }
    });
}

$('#btn-post-memo').on('click', function () {
    const journalId = $(this).data('id');
    Swal.fire({
        title: 'Post Journal?',
        text: "Once posted, this journal cannot be edited or deleted.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Post',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-success', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/MemoJournal/Post',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ journalId: journalId, postedDate: new Date().toISOString() }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-detail').modal('hide');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
                }
            });
        }
    });
});

$('#btn-reverse-memo').on('click', function () {
    const journalId = $(this).data('id');
    Swal.fire({
        title: 'Reverse Journal?',
        html: `
            <p>This will create a reversal journal entry.</p>
            <label>Reversal Date:</label>
            <input type="date" id="reversal-date" class="form-control" value="${new Date().toISOString().split('T')[0]}" />
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Reverse',
        cancelButtonText: 'Cancel',
        customClass: { confirmButton: 'btn btn-danger', cancelButton: 'btn btn-secondary' },
        buttonsStyling: false,
        preConfirm: () => {
            const reversalDate = document.getElementById('reversal-date').value;
            if (!reversalDate) Swal.showValidationMessage('Please select reversal date');
            return reversalDate;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/MemoJournal/Reverse',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ journalId: journalId, reversalDate: result.value }),
                success: function (response) {
                    if (response.success) {
                        Swal.fire('Success', response.message, 'success');
                        $('#modal-detail').modal('hide');
                        dataTable.ajax.reload();
                    }
                },
                error: function (xhr) {
                    Swal.fire('Error', xhr.responseJSON?.message || 'An error occurred', 'error');
                }
            });
        }
    });
});