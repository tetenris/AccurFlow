let currentPreview = null;
let closedYears = [];

$(document).ready(function () {
    $('#btn-preview').on('click', previewYear);
    $('#btn-close-year').on('click', closeYear);
    loadHistory();
});

function previewYear() {
    const fiscalYear = Number($('#fiscal-year').val());
    $.ajax({
        url: '/YearEndClosing/Preview',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ fiscalYear: fiscalYear }),
        success: data => {
            currentPreview = data;
            $('#preview-revenue').text(formatCurrency(data.totalRevenue));
            $('#preview-expense').text(formatCurrency(data.totalExpense));
            $('#preview-net').text(formatCurrency(data.netIncome));
            $('#preview-summary').show();
            $('#preview-content').show();
            $('#preview-revenue-body').html(renderLines(data.revenueAccounts || []));
            $('#preview-expense-body').html(renderLines(data.expenseAccounts || []));
        },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to load preview')
    });
}

function renderLines(lines) {
    if (!lines.length) return '<tr><td colspan="3" class="text-center text-muted">No account to close</td></tr>';
    return lines.map(line => `
        <tr>
            <td>${line.accountCode}</td>
            <td>${line.accountName}</td>
            <td class="text-end">${formatCurrency(line.balance)}</td>
        </tr>`).join('');
}

function closeYear() {
    if (!currentPreview) return;
    if (closedYears.includes(currentPreview.fiscalYear)) { alert(`${currentPreview.fiscalYear} already closed`); return; }
    if (!confirm(`Close fiscal year ${currentPreview.fiscalYear}? Akun P&L akan ditutup ke Retained Earnings via jurnal otomatis.`)) return;
    $.ajax({
        url: '/YearEndClosing/Close',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ fiscalYear: currentPreview.fiscalYear }),
        success: () => { alert(`${currentPreview.fiscalYear} closed successfully`); loadHistory(); previewYear(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to close fiscal year')
    });
}

function loadHistory() {
    $.get('/YearEndClosing/History', data => {
        closedYears = (data || []).map(x => x.fiscalYear);
        const table = $('#history_datatable').DataTable();
        if ($.fn.DataTable.isDataTable('#history_datatable')) {
            table.clear();
            table.rows.add(data || []);
            table.draw();
        } else {
            $('#history_datatable').DataTable({
                data: data || [],
                columns: [
                    { data: 'fiscalYear' },
                    { data: 'closingDate', render: d => new Date(d).toLocaleDateString('en-GB') },
                    { data: 'journalNumber', render: d => d || '-' }
                ]
            });
        }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 2 }).format(amount || 0);
}