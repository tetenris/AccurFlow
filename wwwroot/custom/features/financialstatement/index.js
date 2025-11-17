$(document).ready(function () {
    // Set default dates
    const today = new Date().toISOString().split('T')[0];
    const firstDayOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0];
    
    $('#is-date-from').val(firstDayOfMonth);
    $('#is-date-to').val(today);
    $('#bs-as-of-date').val(today);
    $('#cf-date-from').val(firstDayOfMonth);
    $('#cf-date-to').val(today);

    // Initialize toolbar with Income Statement export button (default active tab)
    updateToolbar('income-statement');

    // Income Statement handlers
    $('#btn-generate-is').on('click', generateIncomeStatement);

    // Balance Sheet handlers
    $('#btn-generate-bs').on('click', generateBalanceSheet);

    // Cash Flow handlers
    $('#btn-generate-cf').on('click', generateCashFlow);

    // Tab change handler
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetTab = $(e.target).data('bs-target').replace('#', '');
        updateToolbar(targetTab);
    });
});

function updateToolbar(activeTab) {
    // Clear toolbar
    $('#toolbar-section-button').empty();
    
    // Create export button with correct base style
    var btnExport = $('<a>', {
        href: 'javascript:void(0)',
        class: 'btn btn-outline btn-outline-primary',
        type: 'button',
        id: 'btn-export-toolbar',
        html: '<i class="fa fa-download"></i> Export Excel'
    });
    
    $('#toolbar-section-button').append(btnExport);
    
    // Attach click handler based on active tab
    $('#btn-export-toolbar').off('click').on('click', function() {
        if (activeTab === 'income-statement') {
            exportIncomeStatement();
        } else if (activeTab === 'balance-sheet') {
            exportBalanceSheet();
        } else if (activeTab === 'cash-flow') {
            exportCashFlow();
        }
    });
}

function generateIncomeStatement() {
    const dateFrom = $('#is-date-from').val();
    const dateTo = $('#is-date-to').val();
    const showZero = $('#is-show-zero').is(':checked');

    if (!dateFrom || !dateTo) {
        Swal.fire('Error', 'Please select date range', 'error');
        return;
    }

    $.ajax({
        url: '/FinancialStatement/GetIncomeStatement',
        type: 'GET',
        data: { dateFrom, dateTo, showZeroBalance: showZero },
        beforeSend: function () {
            $('#income-statement-table').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
        },
        success: function (data) {
            renderIncomeStatementTable(data);
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'Failed to generate report', 'error');
            $('#income-statement-table').html('');
        }
    });
}

function renderIncomeStatementTable(data) {
    let html = '<table class="table table-bordered table-hover">';
    html += '<thead><tr><th>Account Code</th><th>Account Name</th><th class="text-end">Amount</th></tr></thead>';
    html += '<tbody>';

    data.sections.forEach(section => {
        html += `<tr class="table-secondary"><td colspan="3"><strong>${section.sectionName}</strong></td></tr>`;
        
        section.lines.forEach(line => {
            html += `<tr class="clickable-row" data-account-id="${line.accountId}" data-date-from="${data.dateFrom}" data-date-to="${data.dateTo}">`;
            html += `<td>${line.accountCode}</td>`;
            html += `<td>${line.accountName}</td>`;
            html += `<td class="text-end">${formatNumber(line.amount)}</td>`;
            html += '</tr>';
        });

        html += `<tr class="table-light"><td colspan="2"><strong>Total ${section.sectionName}</strong></td>`;
        html += `<td class="text-end"><strong>${formatNumber(section.subtotal)}</strong></td></tr>`;
    });

    html += '<tr class="table-primary"><td colspan="2"><strong>TOTAL REVENUE</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.totalRevenue)}</strong></td></tr>`;
    
    html += '<tr class="table-primary"><td colspan="2"><strong>TOTAL EXPENSE</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.totalExpense)}</strong></td></tr>`;
    
    html += '<tr class="table-success"><td colspan="2"><strong>NET INCOME</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.netIncome)}</strong></td></tr>`;

    html += '</tbody></table>';

    $('#income-statement-table').html(html);

    // Add click handler for drill-down
    $('.clickable-row').on('click', function () {
        const accountId = $(this).data('account-id');
        const dateFrom = $(this).data('date-from');
        const dateTo = $(this).data('date-to');
        window.location.href = `/GeneralLedger/AccountLedger?accountId=${accountId}&dateFrom=${dateFrom}&dateTo=${dateTo}`;
    });
}

function generateBalanceSheet() {
    const asOfDate = $('#bs-as-of-date').val();
    const showZero = $('#bs-show-zero').is(':checked');

    if (!asOfDate) {
        Swal.fire('Error', 'Please select as of date', 'error');
        return;
    }

    $.ajax({
        url: '/FinancialStatement/GetBalanceSheet',
        type: 'GET',
        data: { asOfDate, showZeroBalance: showZero },
        beforeSend: function () {
            $('#balance-sheet-table').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
        },
        success: function (data) {
            renderBalanceSheetTable(data);
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'Failed to generate report', 'error');
            $('#balance-sheet-table').html('');
        }
    });
}

function renderBalanceSheetTable(data) {
    let html = '<table class="table table-bordered table-hover">';
    html += '<thead><tr><th>Account Code</th><th>Account Name</th><th class="text-end">Amount</th></tr></thead>';
    html += '<tbody>';

    data.sections.forEach(section => {
        html += `<tr class="table-secondary"><td colspan="3"><strong>${section.sectionName}</strong></td></tr>`;
        
        section.lines.forEach(line => {
            html += `<tr class="clickable-row" data-account-id="${line.accountId}" data-as-of-date="${data.asOfDate}">`;
            html += `<td>${line.accountCode}</td>`;
            html += `<td>${line.accountName}</td>`;
            html += `<td class="text-end">${formatNumber(line.amount)}</td>`;
            html += '</tr>';
        });

        html += `<tr class="table-light"><td colspan="2"><strong>Total ${section.sectionName}</strong></td>`;
        html += `<td class="text-end"><strong>${formatNumber(section.subtotal)}</strong></td></tr>`;
    });

    html += '<tr class="table-primary"><td colspan="2"><strong>TOTAL LIABILITIES + EQUITY</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.totalLiabilities + data.totalEquity)}</strong></td></tr>`;

    if (!data.isBalanced) {
        html += '<tr class="table-danger"><td colspan="2"><strong>DIFFERENCE (Not Balanced!)</strong></td>';
        html += `<td class="text-end"><strong>${formatNumber(Math.abs(data.difference))}</strong></td></tr>`;
    } else {
        html += '<tr class="table-success"><td colspan="3" class="text-center"><strong>✓ Balanced</strong></td></tr>';
    }

    html += '</tbody></table>';

    $('#balance-sheet-table').html(html);

    // Add click handler for drill-down
    $('.clickable-row').on('click', function () {
        const accountId = $(this).data('account-id');
        const asOfDate = $(this).data('as-of-date');
        window.location.href = `/GeneralLedger/AccountLedger?accountId=${accountId}&dateTo=${asOfDate}`;
    });
}

function generateCashFlow() {
    const dateFrom = $('#cf-date-from').val();
    const dateTo = $('#cf-date-to').val();

    if (!dateFrom || !dateTo) {
        Swal.fire('Error', 'Please select date range', 'error');
        return;
    }

    $.ajax({
        url: '/FinancialStatement/GetCashFlow',
        type: 'GET',
        data: { dateFrom, dateTo },
        beforeSend: function () {
            $('#cash-flow-table').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
        },
        success: function (data) {
            renderCashFlowTable(data);
        },
        error: function (xhr) {
            Swal.fire('Error', xhr.responseJSON?.message || 'Failed to generate report', 'error');
            $('#cash-flow-table').html('');
        }
    });
}

function renderCashFlowTable(data) {
    let html = '<table class="table table-bordered table-hover">';
    html += '<thead><tr><th>Account Code</th><th>Account Name</th><th class="text-end">Amount</th></tr></thead>';
    html += '<tbody>';

    html += '<tr class="table-info"><td colspan="2"><strong>Beginning Cash Balance</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.beginningCashBalance)}</strong></td></tr>`;

    data.sections.forEach(section => {
        html += `<tr class="table-secondary"><td colspan="3"><strong>${section.sectionName}</strong></td></tr>`;
        
        section.lines.forEach(line => {
            html += `<tr class="clickable-row" data-account-id="${line.accountId}" data-date-from="${data.dateFrom}" data-date-to="${data.dateTo}">`;
            html += `<td>${line.accountCode}</td>`;
            html += `<td>${line.accountName}</td>`;
            html += `<td class="text-end">${formatNumber(line.amount)}</td>`;
            html += '</tr>';
        });

        html += `<tr class="table-light"><td colspan="2"><strong>Net Cash from ${section.sectionName}</strong></td>`;
        html += `<td class="text-end"><strong>${formatNumber(section.subtotal)}</strong></td></tr>`;
    });

    html += '<tr class="table-warning"><td colspan="2"><strong>Net Increase (Decrease) in Cash</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.netIncreaseDecrease)}</strong></td></tr>`;

    html += '<tr class="table-success"><td colspan="2"><strong>Ending Cash Balance</strong></td>';
    html += `<td class="text-end"><strong>${formatNumber(data.endingCashBalance)}</strong></td></tr>`;

    html += '</tbody></table>';

    $('#cash-flow-table').html(html);

    // Add click handler for drill-down
    $('.clickable-row').on('click', function () {
        const accountId = $(this).data('account-id');
        const dateFrom = $(this).data('date-from');
        const dateTo = $(this).data('date-to');
        window.location.href = `/GeneralLedger/AccountLedger?accountId=${accountId}&dateFrom=${dateFrom}&dateTo=${dateTo}`;
    });
}

function exportIncomeStatement() {
    const dateFrom = $('#is-date-from').val();
    const dateTo = $('#is-date-to').val();
    const showZero = $('#is-show-zero').is(':checked');

    if (!dateFrom || !dateTo) {
        Swal.fire('Error', 'Please select date range', 'error');
        return;
    }

    window.location.href = `/FinancialStatement/ExportIncomeStatement?dateFrom=${dateFrom}&dateTo=${dateTo}&showZeroBalance=${showZero}`;
}

function exportBalanceSheet() {
    const asOfDate = $('#bs-as-of-date').val();
    const showZero = $('#bs-show-zero').is(':checked');

    if (!asOfDate) {
        Swal.fire('Error', 'Please select as of date', 'error');
        return;
    }

    window.location.href = `/FinancialStatement/ExportBalanceSheet?asOfDate=${asOfDate}&showZeroBalance=${showZero}`;
}

function exportCashFlow() {
    const dateFrom = $('#cf-date-from').val();
    const dateTo = $('#cf-date-to').val();

    if (!dateFrom || !dateTo) {
        Swal.fire('Error', 'Please select date range', 'error');
        return;
    }

    window.location.href = `/FinancialStatement/ExportCashFlow?dateFrom=${dateFrom}&dateTo=${dateTo}`;
}

function formatNumber(num) {
    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(num);
}
