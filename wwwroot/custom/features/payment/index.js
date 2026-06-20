let paymentTable;

$(document).ready(function () {
    paymentTable = $('#payment_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Payment/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                return JSON.stringify({
                    draw: d.draw,
                    search: d.search.value || '',
                    page: (d.start / d.length) + 1,
                    size: d.length,
                    paymentType: $('#filter-payment-type').val() || null,
                    status: $('#filter-status').val() || null
                });
            }
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'paymentNumber' },
            { data: 'paymentType' },
            { data: 'paymentDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'partnerName' },
            { data: 'paymentMethod' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'totalAmount', render: formatCurrency },
            { data: 'paymentId', orderable: false, render: data => `<button class="btn btn-sm btn-light-success btn-post" data-id="${data}">Post</button>` }
        ]
    });

    $('#btn-apply-filter').on('click', () => paymentTable.ajax.reload());
    $('#payment_datatable').on('click', '.btn-post', function () {
        $.ajax({ url: '/Payment/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => paymentTable.ajax.reload() });
    });
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
