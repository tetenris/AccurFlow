let paymentTable;

$(document).ready(function () {
    loadLookups();

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
    $('#btn-add-payment').on('click', openCreateModal);
    $('#btn-save-payment').on('click', savePayment);
    $('#payment-type').on('change', () => { togglePartnerType(); loadOpenInvoices(); });
    $('#payment_datatable').on('click', '.btn-post', function () {
        $.ajax({ url: '/Payment/Post', type: 'POST', contentType: 'application/json', data: JSON.stringify($(this).data('id')), success: () => paymentTable.ajax.reload() });
    });
});

function openCreateModal() {
    $('#allocated-amount').val(0);
    $('#reference-number').val('');
    $('#payment-notes').val('');
    togglePartnerType();
    loadOpenInvoices();
    $('#payment_modal').modal('show');
}

function togglePartnerType() {
    const type = $('#payment-type').val();
    $('#payment-customer-wrapper').toggleClass('d-none', type !== 'Receipt');
    $('#payment-supplier-wrapper').toggleClass('d-none', type !== 'Payment');
}

function loadLookups() {
    $.get('/Customer/GetActiveCustomers', data => fillSelect('#payment-customer-id', data, 'customerId', x => `${x.customerCode} - ${x.customerName}`));
    $.get('/Supplier/GetActiveSuppliers', data => fillSelect('#payment-supplier-id', data, 'supplierId', x => `${x.supplierCode} - ${x.supplierName}`));
    $.get('/ChartOfAccount/GetActiveAccounts', data => fillSelect('#cash-bank-account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function loadOpenInvoices() {
    const invoiceType = $('#payment-type').val() === 'Payment' ? 'Purchase' : 'Sales';
    $.get(`/Invoice/GetOpenInvoices?invoiceType=${invoiceType}`, data => fillSelect('#invoice-id', data || [], 'invoiceId', x => `${x.invoiceNumber} - ${formatCurrency(x.outstandingAmount)}`));
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

function savePayment() {
    const type = $('#payment-type').val();
    const amount = Number($('#allocated-amount').val() || 0);
    const request = {
        paymentType: type,
        paymentDate: $('#payment-date').val(),
        customerId: type === 'Receipt' ? $('#payment-customer-id').val() || null : null,
        supplierId: type === 'Payment' ? $('#payment-supplier-id').val() || null : null,
        cashBankAccountId: $('#cash-bank-account-id').val(),
        paymentMethod: $('#payment-method').val(),
        referenceNumber: $('#reference-number').val(),
        notes: $('#payment-notes').val(),
        allocations: [{ invoiceId: $('#invoice-id').val(), allocatedAmount: amount }]
    };

    $.ajax({
        url: '/Payment/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#payment_modal').modal('hide'); paymentTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save payment')
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
