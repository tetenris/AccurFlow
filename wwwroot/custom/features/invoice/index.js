let invoiceTable;

$(document).ready(function () {
    loadLookups();

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
    $('#btn-add-invoice').on('click', openCreateModal);
    $('#btn-save-invoice').on('click', saveInvoice);
    $('#invoice-type').on('change', togglePartnerType);
    $('#invoice_datatable').on('click', '.btn-post', function () {
        postDocument('/Invoice/Post', $(this).data('id'), invoiceTable);
    });
});

function openCreateModal() {
    $('#line-description').val('');
    $('#quantity').val(1);
    $('#unit-price').val(0);
    $('#discount-amount').val(0);
    $('#tax-amount').val(0);
    $('#notes').val('');
    togglePartnerType();
    $('#invoice_modal').modal('show');
}

function togglePartnerType() {
    const type = $('#invoice-type').val();
    $('#customer-wrapper').toggleClass('d-none', type !== 'Sales');
    $('#supplier-wrapper').toggleClass('d-none', type !== 'Purchase');
}

function loadLookups() {
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
    $.get('/ChartOfAccount/GetActiveAccounts', data => fillSelect('#account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function saveInvoice() {
    const type = $('#invoice-type').val();
    const request = {
        invoiceType: type,
        invoiceDate: $('#invoice-date').val(),
        dueDate: $('#due-date').val(),
        customerId: type === 'Sales' ? $('#customer-id').val() || null : null,
        supplierId: type === 'Purchase' ? $('#supplier-id').val() || null : null,
        notes: $('#notes').val(),
        lines: [{
            accountId: $('#account-id').val() || null,
            description: $('#line-description').val(),
            quantity: Number($('#quantity').val() || 0),
            unitPrice: Number($('#unit-price').val() || 0),
            discountAmount: Number($('#discount-amount').val() || 0),
            taxAmount: Number($('#tax-amount').val() || 0)
        }]
    };

    $.ajax({
        url: '/Invoice/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#invoice_modal').modal('hide'); invoiceTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save invoice')
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}

function postDocument(url, id, table) {
    $.ajax({ url, type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => table.ajax.reload() });
}
