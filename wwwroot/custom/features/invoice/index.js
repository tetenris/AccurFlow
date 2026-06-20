let invoiceTable;

$(document).ready(function () {
    invoiceTable = $('#invoice_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Invoice/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    draw: d.draw,
                    search: d.search.value || '',
                    page: (d.start / d.length) + 1,
                    size: d.length,
                    invoiceType: $('#filter-invoice-type').val() || null,
                    status: $('#filter-status').val() || null
                });
            }
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'invoiceNumber' },
            { data: 'invoiceType' },
            { data: 'invoiceDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'dueDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'partnerName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'totalAmount', render: formatCurrency },
            { data: 'outstandingAmount', render: formatCurrency },
            { data: 'invoiceId', orderable: false, render: data => `<button class="btn btn-sm btn-light-success btn-post" data-id="${data}">Post</button>` }
        ]
    });

    $('#btn-apply-filter').on('click', () => invoiceTable.ajax.reload());
    $('#invoice_datatable').on('click', '.btn-post', function () {
        postDocument('/Invoice/Post', $(this).data('id'), invoiceTable);
    });
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}

function postDocument(url, id, table) {
    $.ajax({ url, type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => table.ajax.reload() });
}
