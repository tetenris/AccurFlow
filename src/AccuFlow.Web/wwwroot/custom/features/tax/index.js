let currentTaxId = null;
let vatTable;

$(document).ready(function () {
    initTax();
    initVatReport();
});

function initTax() {
    window.taxTable = $('#tax_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Tax/Datatable',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({ draw: d.draw, search: d.search.value || '', page: (d.start / d.length) + 1, size: d.length })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'taxCode' },
            { data: 'taxName' },
            { data: 'rate' },
            { data: 'taxType' },
            { data: 'isActive', render: data => data ? '<span class="badge badge-light-success">Active</span>' : '<span class="badge badge-light-warning">Inactive</span>' },
            { data: null, orderable: false, render: taxActionButtons }
        ]
    });

    $('#btn-add-tax').on('click', openCreateModal);
    $('#btn-save-tax').on('click', saveTax);
    $('#tax_datatable').on('click', '.btn-tax-edit', function () { openEditModal($(this).data('id')); });
    $('#tax_datatable').on('click', '.btn-tax-delete', function () { deleteTax($(this).data('id')); });
}

function initVatReport() {
    vatTable = $('#vat_datatable').DataTable({
        data: [],
        columns: [
            { data: 'invoiceNumber' },
            { data: 'invoiceType' },
            { data: 'partnerName' },
            { data: 'invoiceDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'dpp', render: formatCurrency },
            { data: 'vat', render: formatCurrency },
            { data: 'totalAmount', render: formatCurrency }
        ]
    });
    $('#btn-generate-vat').on('click', generateVatReport);
}

function taxActionButtons(row) {
    return `<div class="d-flex gap-1">
        <button class="btn btn-sm btn-light-primary btn-tax-edit" data-id="${row.taxId}">Edit</button>
        <button class="btn btn-sm btn-light-danger btn-tax-delete" data-id="${row.taxId}">Delete</button>
    </div>`;
}

function openCreateModal() {
    currentTaxId = null;
    $('#tax-code').val('');
    $('#tax-name').val('');
    $('#tax-rate').val('');
    $('#tax-type').val('VAT');
    $('#tax-active').prop('checked', true);
    $('#tax_modal').modal('show');
}

function openEditModal(id) {
    $.get(`/Tax/GetById?id=${id}`, data => {
        currentTaxId = data.taxId;
        $('#tax-code').val(data.taxCode);
        $('#tax-name').val(data.taxName);
        $('#tax-rate').val(data.rate);
        $('#tax-type').val(data.taxType);
        $('#tax-active').prop('checked', data.isActive);
        $('#tax_modal').modal('show');
    });
}

function saveTax() {
    const request = {
        taxCode: $('#tax-code').val(),
        taxName: $('#tax-name').val(),
        rate: Number($('#tax-rate').val() || 0),
        taxType: $('#tax-type').val(),
        isActive: $('#tax-active').is(':checked')
    };
    if (currentTaxId) request.taxId = currentTaxId;

    $.ajax({
        url: currentTaxId ? '/Tax/Edit' : '/Tax/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#tax_modal').modal('hide'); window.taxTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save tax')
    });
}

function deleteTax(id) {
    if (!confirm('Delete this tax?')) return;
    $.ajax({ url: '/Tax/Delete', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.taxTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete tax') });
}

function generateVatReport() {
    $.ajax({
        url: '/Tax/VatReport', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ fromDate: $('#vat-from').val(), toDate: $('#vat-to').val() }),
        success: data => {
            if (!data.data) return;
            const { lines, summary } = data.data;
            $('#vat-output').text(formatCurrency(summary.outputVat));
            $('#vat-input').text(formatCurrency(summary.inputVat));
            $('#vat-net').text(formatCurrency(summary.netVat));
            $('#vat-summary').show();
            vatTable.clear();
            vatTable.rows.add(lines || []);
            vatTable.draw();
        }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}