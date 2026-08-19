let rpTable;

$(document).ready(function () {
    rpTable = $('#receivable_payable_datatable').DataTable({
        data: [],
        columns: [
            { data: 'partnerCode' },
            { data: 'partnerName' },
            { data: 'invoiceNumber' },
            { data: 'invoiceDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'dueDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'totalAmount', render: formatCurrency },
            { data: 'paidAmount', render: formatCurrency },
            { data: 'outstandingAmount', render: formatCurrency },
            { data: 'status', render: data => data === 'Overdue' ? '<span class="badge badge-light-danger">Overdue</span>' : '<span class="badge badge-light-success">Open</span>' }
        ]
    });
    $('#btn-generate').on('click', generateReport);
    generateReport();
});

function generateReport() {
    $.ajax({
        url: '/ReceivablePayable/Generate', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ reportType: $('#report-type').val(), asOfDate: $('#as-of-date').val() }),
        success: data => { rpTable.clear(); rpTable.rows.add(data); rpTable.draw(); }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}