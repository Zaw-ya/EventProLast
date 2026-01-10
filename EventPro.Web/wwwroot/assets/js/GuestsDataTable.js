$(document).ready(function () {
    console.log(@ViewBag.EventId);
    $('#EventsTable').DataTable({
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
        "language": {
            'loadingRecords': 'Processing...',
        },
        "filter": true,
        "ajax": {
            "url": "/api/GetGuests?id=" + Eventid,
            "type": "POST",
            "dataType" : "json"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "render": function (data, type, row) { }, "autowidth": true, "orderable": false, "data": null },
            { "render": function (data, type, row) { }, "autowidth": true, "orderable": false, "data": null },
            { "data": "id", "name": "Id", "autowidth": true },
            { "data": "id", "name": "Id", "autowidth": true },
            { "data": "EventId", "name": "EventId", "autowidth": true },
            { "data": "name", "name": "Name", "autowidth": true },
            { "data": "phoneNumber", "name": "PhoneNumber", "autowidth": true },
            { "data": "scannedInfo", "name": "ScannedInfo", "autowidth": true },
            { "data": "confirmationMsgStatus", "name": "ConfirmationMsgStatus", "autowidth": true },
            { "data": "cardMsgStatus", "name": "CardMsgStatus", "autowidth": true },
            { "data": "eventLocationMsgStatus", "name": "EventLocationMsgStatus", "autowidth": true, orderable: false },
            { "data": "reminderMsgStatus", "name": "ReminderMsgStatus", "autowidth": true, "orderable": false },
            { "data": "congratulationMsgStatus", "name": "CongratulationMsgStatus", "autowidth": true, "orderable": false },
            { "data": "response", "name": "Response", "autowidth": true, "orderable": false },
            { "data": "responseTime", "name": "ResponseTime", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {
                 

                   

                           
                    
                },
                "orderable": false,
                "data": null
            }]

    });
    $('#RefreshPage').click(function () {
        window.location.reload();
    });
    $('#SearchBtn').click(function () {
        dataTable.draw();
    });
} )