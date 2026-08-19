let agingTable;

$(document).ready(function () {
    agingTable = $('#aging_datatable').DataTable({ data: [], columns: [
        { data: 'partnerCode' }, { data: 'partnerName' }, { data: 'current', render: formatCurrency },
        { data: 'days1To30', render: formatCurrency }, { data: 'days31To60', render: formatCurrency },
        { data: 'days61To90', render: formatCurrency }, { data: 'over90', render: formatCurrency }, { data: 'total', render: formatCurrency }
    ]});
    $('#btn-generate').on('click', generateAging);
    generateAging();
});

function generateAging() {
    $.ajax({
        url: '/AgingReport/Generate', type: 'POST', contentType: 'application/json',
        data: JSON.stringify({ agingType: $('#aging-type').val(), asOfDate: $('#as-of-date').val() }),
        success: data => { agingTable.clear(); agingTable.rows.add(data); agingTable.draw(); }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', minimumFractionDigits: 0 }).format(amount || 0);
}
