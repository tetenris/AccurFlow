$(document).ready(function () {
    initializeEventHandlers();
    generateTrialBalance();
});

function initializeEventHandlers() {
    $('#btn-generate').on('click', function () {
        generateTrialBalance();
    });

    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-excel',
        html: '<i class="fa fa-download"></i> Export Excel'
    });
    $('#toolbar-section-button').append(btnExport);

    $(document).on('click', '#btn-export-excel', function () {
        exportToExcel();
    });
}

function generateTrialBalance() {
    const params = new URLSearchParams({
        asOfDate: $('#filter-as-of-date').val() || '',
        accountType: $('#filter-account-type').val() || '',
        showZeroBalance: $('#filter-show-zero').is(':checked')
    });

    $.ajax({
        url: `/TrialBalance/GetTrialBalance?${params.toString()}`,
        type: 'GET',
        success: function (data) {
            displayTrialBalance(data);
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'Failed to generate trial balance', 'error');
        }
    });
}

function displayTrialBalance(data) {
    $('#report-date').text(formatDate(data.asOfDate));

    const tableContainer = $('#trial-balance-table');
    tableContainer.empty();

    if (!data.groups || data.groups.length === 0) {
        tableContainer.html('<div class="alert alert-warning">No accounts found for the selected criteria</div>');
        $('#trial-balance-result').show();
        return;
    }

    let tableHtml = '<table class="table table-striped table-bordered">';
    tableHtml += '<thead><tr><th>Account Code</th><th>Account Name</th><th class="text-end">Debit</th><th class="text-end">Credit</th></tr></thead>';
    tableHtml += '<tbody>';

    data.groups.forEach(function (group) {
        tableHtml += `<tr class="table-secondary"><td colspan="4"><strong>Account Type: ${group.accountType}</strong></td></tr>`;

        group.accounts.forEach(function (account) {
            tableHtml += `
                <tr class="account-row" data-account-id="${account.accountId}" style="cursor:pointer;">
                    <td>${account.accountCode}</td>
                    <td>${account.accountName}</td>
                    <td class="text-end">${formatCurrency(account.debitBalance)}</td>
                    <td class="text-end">${formatCurrency(account.creditBalance)}</td>
                </tr>
            `;
        });

        tableHtml += `
            <tr class="table-light fw-bold">
                <td colspan="2" class="text-end">Subtotal</td>
                <td class="text-end">${formatCurrency(group.subtotalDebit)}</td>
                <td class="text-end">${formatCurrency(group.subtotalCredit)}</td>
            </tr>
        `;
    });

    tableHtml += '</tbody></table>';
    tableContainer.html(tableHtml);

    $('#total-debit').text(formatCurrency(data.totalDebit));
    $('#total-credit').text(formatCurrency(data.totalCredit));

    if (data.isBalanced) {
        $('#difference-row').hide();
        $('#balance-status').html('<span class="badge badge-success">✓ BALANCED</span>');
    } else {
        $('#difference-row').show();
        $('#difference-amount').text(formatCurrency(Math.abs(data.difference)));
        $('#balance-status').html('<span class="badge badge-danger">✗ NOT BALANCED</span>');
    }

    $('#trial-balance-result').show();

    $('.account-row').on('click', function () {
        const accountId = $(this).data('account-id');
        drillDownToLedger(accountId);
    });
}

function drillDownToLedger(accountId) {
    const asOfDate = $('#filter-as-of-date').val();
    const url = `/GeneralLedger/AccountLedger?accountId=${accountId}`;
    window.location.href = url;
}

function exportToExcel() {
    const params = new URLSearchParams({
        asOfDate: $('#filter-as-of-date').val() || '',
        accountType: $('#filter-account-type').val() || '',
        showZeroBalance: $('#filter-show-zero').is(':checked')
    });

    window.location.href = `/TrialBalance/ExportExcel?${params.toString()}`;
}

function formatCurrency(value) {
    if (value === 0) return '0.00';
    return value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB');
}
