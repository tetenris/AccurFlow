$(document).ready(function () {
    const accountId = $('#account-id').val();
    loadLedgerData(accountId);
    initializeEventHandlers(accountId);
});

function loadLedgerData(accountId) {
    const dateFrom = $('#filter-date-from').val();
    const dateTo = $('#filter-date-to').val();

    const params = new URLSearchParams({
        accountId: accountId,
        dateFrom: dateFrom || '',
        dateTo: dateTo || ''
    });

    $.ajax({
        url: `/GeneralLedger/GetLedger?${params.toString()}`,
        type: 'GET',
        success: function (data) {
            $('#page-title').text(`Account Ledger - ${data.accountCode} ${data.accountName}`);

            if (data.openingBalance !== 0) {
                $('#opening-balance-section').show();
                $('#opening-balance').text(formatCurrency(data.openingBalance));
            } else {
                $('#opening-balance-section').hide();
            }

            const tbody = $('#ledger_datatable tbody');
            tbody.empty();

            if (data.entries && data.entries.length > 0) {
                data.entries.forEach(function (entry) {
                    let statusBadge = '';
                    if (entry.status === 'Reversed') {
                        statusBadge = '<span class="badge badge-light-danger ms-2">Reversed</span>';
                    } else if (entry.isReversal) {
                        statusBadge = '<span class="badge badge-light-info ms-2">Reversal Entry</span>';
                    }
                    
                    const row = `
                        <tr>
                            <td>${formatDate(entry.journalDate)}</td>
                            <td><a href="/JournalEntry/Index" class="text-primary">${entry.journalNumber}</a>${statusBadge}</td>
                            <td>${entry.description}</td>
                            <td class="text-end">${formatCurrency(entry.debitAmount)}</td>
                            <td class="text-end">${formatCurrency(entry.creditAmount)}</td>
                            <td class="text-end fw-bold">${formatCurrency(entry.runningBalance)}</td>
                        </tr>
                    `;
                    tbody.append(row);
                });
            } else {
                tbody.append('<tr><td colspan="6" class="text-center">No transactions found</td></tr>');
            }

            $('#total-debit').text(formatCurrency(data.totalDebit));
            $('#total-credit').text(formatCurrency(data.totalCredit));
            $('#closing-balance').text(formatCurrency(data.closingBalance));
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'Failed to load ledger data', 'error');
        }
    });
}

function initializeEventHandlers(accountId) {
    $('#btn-apply-filter').on('click', function () {
        loadLedgerData(accountId);
    });

    var btnBack = $('<a>', {
        href: '/GeneralLedger/Index',
        class: 'btn btn-outline btn-outline-secondary me-2',
        html: '<i class="fa fa-arrow-left"></i> Back to Summary'
    });
    $('#toolbar-section-button').append(btnBack);

    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-ledger',
        html: '<i class="fa fa-download"></i> Export'
    });
    $('#toolbar-section-button').append(btnExport);

    $(document).on('click', '#btn-export-ledger', function () {
        exportLedger(accountId);
    });
}

function exportLedger(accountId) {
    const params = new URLSearchParams({
        accountId: accountId,
        dateFrom: $('#filter-date-from').val() || '',
        dateTo: $('#filter-date-to').val() || ''
    });

    window.location.href = `/GeneralLedger/ExportLedger?${params.toString()}`;
}

function formatCurrency(value) {
    if (value === 0) return '0.00';
    const formatted = Math.abs(value).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return value < 0 ? `(${formatted})` : formatted;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB');
}
