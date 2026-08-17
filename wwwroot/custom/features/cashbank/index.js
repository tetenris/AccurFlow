$(document).ready(function () {
    if ($('#transfer_datatable').length) initTransfers();
    if ($('#reconciliation_datatable').length) initReconciliations();
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}

function fillSelect(selector, data, valueField, textFactory) {
    const select = $(selector);
    select.empty();
    select.append('<option value="">Select</option>');
    (data || []).forEach(item => select.append(`<option value="${item[valueField]}">${textFactory(item)}</option>`));
}

/* ============================ Transfers ============================ */

let currentTransferId = null;

function initTransfers() {
    window.transferTable = $('#transfer_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/CashBank/DatatableTransfers',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({
                draw: d.draw,
                search: d.search.value || '',
                page: (d.start / d.length) + 1,
                size: d.length,
                status: $('#filter-status').val() || null
            })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'transferNumber' },
            { data: 'transferDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'fromAccountName' },
            { data: 'toAccountName' },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: 'amount', render: formatCurrency },
            { data: null, orderable: false, render: transferActionButtons }
        ]
    });

    $('#btn-apply-filter').on('click', () => window.transferTable.ajax.reload());
    $('#btn-add-transfer').on('click', openTransferCreateModal);
    $('#btn-save-transfer').on('click', saveTransfer);
    $('#transfer_datatable').on('click', '.btn-tpost', function () { postTransfer($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-tdetail', function () { showTransferDetail($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-tedit', function () { openTransferEditModal($(this).data('id')); });
    $('#transfer_datatable').on('click', '.btn-tdelete', function () { deleteTransfer($(this).data('id')); });

    loadCashAccounts();
    loadBankAccounts();
}

function transferActionButtons(row) {
    const id = row.transferId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-tedit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-tdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-tpost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-tdetail" data-id="${id}">Detail</button>${draft}</div>`;
}

function loadCashAccounts() {
    $.get('/CashBank/GetCashAccounts', data => fillSelect('#from-account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function loadBankAccounts() {
    $.get('/CashBank/GetBankAccounts', data => fillSelect('#to-account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function openTransferCreateModal() {
    currentTransferId = null;
    $('#transfer-date').val(new Date().toISOString().substring(0, 10));
    $('#transfer-amount').val(0);
    $('#reference-number').val('');
    $('#transfer-description').val('');
    $('#from-account-id').val('');
    $('#to-account-id').val('');
    $('#transfer_modal').modal('show');
}

function openTransferEditModal(id) {
    $.get(`/CashBank/GetTransferDetail?id=${id}`, data => {
        currentTransferId = data.transferId;
        $('#transfer-date').val(data.transferDate?.substring(0, 10));
        $('#from-account-id').val(data.fromAccountId);
        $('#to-account-id').val(data.toAccountId);
        $('#transfer-amount').val(data.amount);
        $('#reference-number').val(data.referenceNumber || '');
        $('#transfer-description').val(data.description || '');
        $('#transfer_modal').modal('show');
    });
}

function saveTransfer() {
    const request = {
        transferDate: $('#transfer-date').val(),
        fromAccountId: $('#from-account-id').val(),
        toAccountId: $('#to-account-id').val(),
        amount: Number($('#transfer-amount').val() || 0),
        description: $('#transfer-description').val(),
        referenceNumber: $('#reference-number').val()
    };
    if (currentTransferId) request.transferId = currentTransferId;

    $.ajax({
        url: currentTransferId ? '/CashBank/UpdateTransfer' : '/CashBank/CreateTransfer',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#transfer_modal').modal('hide'); window.transferTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save transfer')
    });
}

function postTransfer(id) {
    if (!confirm('Post this transfer? A journal entry will be created.')) return;
    $.ajax({ url: '/CashBank/PostTransfer', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.transferTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post transfer') });
}

function deleteTransfer(id) {
    if (!confirm('Delete this draft transfer?')) return;
    $.ajax({ url: '/CashBank/DeleteTransfer', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.transferTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete transfer') });
}

function showTransferDetail(id) {
    $.get(`/CashBank/GetTransferDetail?id=${id}`, data => {
        $('#transfer-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.transferNumber}</div>
                <div class="col-md-4"><strong>Date:</strong> ${new Date(data.transferDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>From:</strong> ${data.fromAccountName}</div>
                <div class="col-md-4"><strong>To:</strong> ${data.toAccountName}</div>
                <div class="col-md-4"><strong>Journal:</strong> ${data.journalNumber || '-'}</div>
                <div class="col-md-4"><strong>Reference:</strong> ${data.referenceNumber || '-'}</div>
                <div class="col-md-8"><strong>Description:</strong> ${data.description || '-'}</div>
            </div>
            <div class="text-end fw-bold mb-5">Amount: ${formatCurrency(data.amount)}</div>`);
        $('#transfer_detail_modal').modal('show');
    });
}

/* ============================ Reconciliations ============================ */

let currentReconciliationId = null;

function initReconciliations() {
    window.reconTable = $('#reconciliation_datatable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/CashBank/DatatableReconciliations',
            type: 'POST',
            contentType: 'application/json',
            data: d => JSON.stringify({
                draw: d.draw,
                search: d.search.value || '',
                page: (d.start / d.length) + 1,
                size: d.length,
                status: $('#filter-rstatus').val() || null
            })
        },
        columns: [
            { data: null, orderable: false, render: (data, type, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
            { data: 'reconciliationNumber' },
            { data: 'accountName' },
            { data: 'statementDate', render: data => new Date(data).toLocaleDateString('en-GB') },
            { data: 'statementEndingBalance', render: formatCurrency },
            { data: 'glEndingBalance', render: formatCurrency },
            { data: null, render: row => `${row.lineCount} (${row.clearedCount})` },
            { data: 'status', render: data => `<span class="badge badge-light-primary">${data}</span>` },
            { data: null, orderable: false, render: reconActionButtons }
        ]
    });

    $('#btn-apply-rfilter').on('click', () => window.reconTable.ajax.reload());
    $('#btn-add-reconciliation').on('click', openReconCreateModal);
    $('#btn-save-reconciliation').on('click', saveReconciliation);
    $('#btn-add-recon-line').on('click', () => addReconLine());
    $('#btn-load-statement').on('click', loadBankStatement);
    $('#recon-lines-body').on('click', '.btn-remove-recon-line', function () { $(this).closest('tr').remove(); updateReconBalancePreview(); });
    $('#recon-lines-body').on('change', '.recon-line-cleared, .recon-line-amount', updateReconBalancePreview);
    $('#reconciliation_datatable').on('click', '.btn-rpost', function () { postReconciliation($(this).data('id')); });
    $('#reconciliation_datatable').on('click', '.btn-rdetail', function () { showReconciliationDetail($(this).data('id')); });
    $('#reconciliation_datatable').on('click', '.btn-redit', function () { openReconEditModal($(this).data('id')); });
    $('#reconciliation_datatable').on('click', '.btn-rdelete', function () { deleteReconciliation($(this).data('id')); });

    loadReconAccounts();
}

function reconActionButtons(row) {
    const id = row.reconciliationId;
    const draft = row.status === 'Draft'
        ? `<button class="btn btn-sm btn-light-primary btn-redit" data-id="${id}">Edit</button> <button class="btn btn-sm btn-light-danger btn-rdelete" data-id="${id}">Delete</button> <button class="btn btn-sm btn-light-success btn-rpost" data-id="${id}">Post</button>`
        : '';
    return `<div class="d-flex gap-1"><button class="btn btn-sm btn-light-info btn-rdetail" data-id="${id}">Detail</button>${draft}</div>`;
}

function loadReconAccounts() {
    $.get('/CashBank/GetBankAccounts', data => fillSelect('#recon-account-id', data, 'accountId', x => `${x.accountCode} - ${x.accountName}`));
}

function openReconCreateModal() {
    currentReconciliationId = null;
    $('#recon-account-id').val('');
    $('#recon-statement-date').val(new Date().toISOString().substring(0, 10));
    $('#recon-statement-balance').val(0);
    $('#recon-gl-balance').val(0);
    $('#recon-notes').val('');
    $('#recon-lines-body').empty();
    $('#recon-balance-alert').addClass('d-none');
    $('#reconciliation_modal').modal('show');
}

function openReconEditModal(id) {
    $.get(`/CashBank/GetReconciliationDetail?id=${id}`, data => {
        currentReconciliationId = data.reconciliationId;
        $('#recon-account-id').val(data.accountId);
        $('#recon-statement-date').val(data.statementDate?.substring(0, 10));
        $('#recon-statement-balance').val(data.statementEndingBalance);
        $('#recon-gl-balance').val(data.glEndingBalance);
        $('#recon-notes').val(data.notes || '');
        $('#recon-lines-body').empty();
        (data.lines || []).forEach(line => addReconLine(line));
        updateReconBalancePreview();
        $('#reconciliation_modal').modal('show');
    });
}

function addReconLine(line) {
    line = line || {};
    const html = `
        <tr>
            <td><input type="date" class="form-control form-control-sm recon-line-date" value="${line.transactionDate?.substring(0, 10) || new Date().toISOString().substring(0, 10)}" /></td>
            <td><input type="text" class="form-control form-control-sm recon-line-doc" value="${line.documentNumber || ''}" /></td>
            <td><input type="text" class="form-control form-control-sm recon-line-desc" value="${line.description || ''}" /></td>
            <td><input type="number" step="0.01" class="form-control form-control-sm text-end recon-line-amount" value="${line.amount || 0}" /></td>
            <td class="text-center"><input type="checkbox" class="form-check-input recon-line-cleared" ${line.isCleared === false ? '' : 'checked'} /></td>
            <td><button type="button" class="btn btn-sm btn-light-danger btn-remove-recon-line">Remove</button></td>
        </tr>`;
    $('#recon-lines-body').append(html);
}

function updateReconBalancePreview() {
    const gl = Number($('#recon-gl-balance').val() || 0);
    let cleared = 0, float = 0;
    $('#recon-lines-body tr').each(function () {
        const amount = Number($(this).find('.recon-line-amount').val() || 0);
        const isCleared = $(this).find('.recon-line-cleared').is(':checked');
        if (isCleared) cleared += amount; else float += amount;
    });
    const statement = Number($('#recon-statement-balance').val() || 0);
    const computed = gl + cleared - float;
    const alert = $('#recon-balance-alert');
    if (Math.abs(computed - statement) > 0.01) {
        alert.removeClass('d-none').text(`Not balanced. GL ${formatCurrency(gl)} + Cleared ${formatCurrency(cleared)} - Float ${formatCurrency(float)} = ${formatCurrency(computed)} vs Statement ${formatCurrency(statement)}`);
    } else {
        alert.addClass('d-none');
    }
}

function loadBankStatement() {
    const accountId = $('#recon-account-id').val();
    const asOfDate = $('#recon-statement-date').val();
    if (!accountId) return alert('Select a bank account first');
    $.get(`/CashBank/GetBankStatement?accountId=${accountId}&asOfDate=${asOfDate}`, data => {
        const glTotal = (data || []).reduce((sum, l) => sum + (l.amount || 0), 0);
        $('#recon-gl-balance').val(glTotal);
        $('#recon-lines-body').empty();
        (data || []).forEach(line => addReconLine(line));
        updateReconBalancePreview();
    });
}

function collectReconLines() {
    const lines = [];
    $('#recon-lines-body tr').each(function () {
        lines.push({
            transactionDate: $(this).find('.recon-line-date').val(),
            documentNumber: $(this).find('.recon-line-doc').val(),
            description: $(this).find('.recon-line-desc').val(),
            amount: Number($(this).find('.recon-line-amount').val() || 0),
            isCleared: $(this).find('.recon-line-cleared').is(':checked')
        });
    });
    return lines;
}

function saveReconciliation() {
    const request = {
        accountId: $('#recon-account-id').val(),
        statementDate: $('#recon-statement-date').val(),
        statementEndingBalance: Number($('#recon-statement-balance').val() || 0),
        glEndingBalance: Number($('#recon-gl-balance').val() || 0),
        notes: $('#recon-notes').val(),
        lines: collectReconLines()
    };
    if (currentReconciliationId) request.reconciliationId = currentReconciliationId;

    $.ajax({
        url: currentReconciliationId ? '/CashBank/UpdateReconciliation' : '/CashBank/CreateReconciliation',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: () => { $('#reconciliation_modal').modal('hide'); window.reconTable.ajax.reload(); },
        error: xhr => alert(xhr.responseJSON?.message || 'Failed to save reconciliation')
    });
}

function postReconciliation(id) {
    if (!confirm('Post this reconciliation?')) return;
    $.ajax({ url: '/CashBank/PostReconciliation', type: 'POST', contentType: 'application/json', data: JSON.stringify(id), success: () => window.reconTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to post reconciliation') });
}

function deleteReconciliation(id) {
    if (!confirm('Delete this draft reconciliation?')) return;
    $.ajax({ url: '/CashBank/DeleteReconciliation', type: 'DELETE', contentType: 'application/json', data: JSON.stringify(id), success: () => window.reconTable.ajax.reload(), error: xhr => alert(xhr.responseJSON?.message || 'Failed to delete reconciliation') });
}

function showReconciliationDetail(id) {
    $.get(`/CashBank/GetReconciliationDetail?id=${id}`, data => {
        const rows = (data.lines || []).map(l => `
            <tr>
                <td>${new Date(l.transactionDate).toLocaleDateString('en-GB')}</td>
                <td>${l.documentNumber}</td>
                <td>${l.description}</td>
                <td class="text-end">${formatCurrency(l.amount)}</td>
                <td class="text-center">${l.isCleared ? '<span class="badge badge-light-success">Cleared</span>' : '<span class="badge badge-light-warning">Float</span>'}</td>
            </tr>`).join('');
        $('#reconciliation-detail-content').html(`
            <div class="row g-3 mb-4">
                <div class="col-md-4"><strong>No:</strong> ${data.reconciliationNumber}</div>
                <div class="col-md-4"><strong>Account:</strong> ${data.accountName}</div>
                <div class="col-md-4"><strong>Status:</strong> ${data.status}</div>
                <div class="col-md-4"><strong>Statement Date:</strong> ${new Date(data.statementDate).toLocaleDateString('en-GB')}</div>
                <div class="col-md-4"><strong>Statement Balance:</strong> ${formatCurrency(data.statementEndingBalance)}</div>
                <div class="col-md-4"><strong>GL Balance:</strong> ${formatCurrency(data.glEndingBalance)}</div>
                <div class="col-md-12"><strong>Notes:</strong> ${data.notes || '-'}</div>
            </div>
            <table class="table table-sm table-bordered">
                <thead><tr><th>Date</th><th>Document No</th><th>Description</th><th>Amount</th><th>Status</th></tr></thead>
                <tbody>${rows || '<tr><td colspan="5" class="text-center text-muted">No lines</td></tr>'}</tbody>
            </table>
            <div class="text-end fw-bold mb-5">Difference: ${formatCurrency(data.statementEndingBalance - data.glEndingBalance)}</div>`);
        $('#reconciliation_detail_modal').modal('show');
    });
}