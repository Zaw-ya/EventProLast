$(document).ready(function () {
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
        "filter": true,
        "ajax": {
            "url": "/api/GetAvaliableGateKeepers?eventId+@EventId",
            "type": "POST",
            "dataType" : "json"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "name", "name": "Name", "autowidth": true },
            { "data": "name", "name": "Name", "autowidth": true },
            {
                "render": function (data, type, row) {
                    let output = "<ul>";
                    for (let i = 0; i < row.AssignedEventsOnSameDay.length; i++) {
                        output += "<li>"+row.AssignedEventsOnSameDay[0]+"</li>";
                    }
                    output += "</ul>";

                    return output;
                },
                "orderable": false,
                "data": null
            }],
        "columns": [
            { "data": "name", "name": "Name", "autowidth": true },
            { "data": "name", "name": "Name", "autowidth": true },
            {
                "render": function (data, type, row) {
                    return ` <td><a href="~/admin/assigned?userid=${row.Id}&eventId=${ViewBag.EventId}" class="btn btn-success">&nbsp; Assigned</a></td>`
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