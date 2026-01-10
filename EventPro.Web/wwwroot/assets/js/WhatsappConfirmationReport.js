$(document).ready(function () {
    $('#WhatsappConfirmationReport').DataTable({
        "fixedHeader": false,
        "lengthChange": true,
        "searching": true,
        "ordering": true,
        "info": true,
        "autoWidth": false,
        "responsive": true,
        dom: 'lfrBtip',
        buttons: [
            'copy', 'csv', 'excel', 'print'
        ],
        "serverSide": true,
        "processing": true,
        "filter": true,
        "ajax": {
            "url": "/admin/getWaRespReport",
            "type": "POST",
            "dataType" : "json"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "eventCode", "name": "EventCode", "autowidth": true, "orderable": false },
            { "data": "eventCode", "name": "EventCode", "autowidth": true, "orderable": false },
            { "data": "linkedTo", "name": "LinkedTo", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {
                    if (userRole === 'agent') {
                        return `${row.eventTitle}`;
                    } else {
                        return `<a href="/admin/guests/${row.eventId}">${row.eventTitle}</a>`;
                    }
                },
                "orderable": false,
                "data": null
            },
            { "data": "sent", "name": "Sent", "autowidth": true, "orderable": false },
            { "data": "countYes", "name": "CountYes", "autowidth": true, "orderable": false },
            { "data": "confirmedGuests", "name": "ConfirmedGuests", "autowidth": true, "orderable": false },
            { "data": "countNo", "name": "CountNo", "autowidth": true, "orderable": false },
            { "data": "waiting", "name": "Waiting", "autowidth": true, orderable: false, "orderable": false }
            ]

    });
    $('#RefreshPage').click(function () {
        window.location.reload();
    });
    $('#SearchBtn').click(function () {
        dataTable.draw();
    });
} )