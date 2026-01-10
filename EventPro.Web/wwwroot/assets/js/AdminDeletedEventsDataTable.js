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
            "url": "/Admin/GetDeletedEvents",
            "type": "POST",
            "dataType" : "json"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "id", "name": "Id", "autowidth": true },
            { "data": "id", "name": "Id", "autowidth": true },
            { "data": "linked_To", "name": "LinkedEvent", "autowidth": true },
            { "data": "title", "name": "SystemEventTitle", "autowidth": true },
            { "data": "start_Date", "name": "EventFrom", "autowidth": true },
            { "data": "end_Date", "name": "EventTo", "autowidth": true },
            { "data": "venue", "name": "EventVenue", "autowidth": true },
            {
                "data": "deleted_On",
                "name": "DeletedOn",
                "autowidth": true,
                "render": function (data, type, row) {
                    if (!data) return "";
                    let localDate = new Date(data);
                    return localDate.toLocaleString();
                }
            },            { "data": "deleted_By", "name": "Deleted_By", "autowidth": true, orderable: false },
            { "data": "status", "name": "Status", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {
                    
                    return `
                        <div class="btn-group">

                            <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                               Action
                            </button>

                        <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">

                        <a href="#" class="dropdown-item  restore-event" data-id="${row.id}">
                             <i class="nav-icon fas fa-undo" style="color:cornflowerblue"></i>&nbsp;
                              Restore Event
                        </a>
           
                    `;
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
    $(document).on("click", ".restore-event", function (e) {
        e.preventDefault();
        var eventId = $(this).data("id");

        if (confirm("Are you sure you want to restore this event?")) {
            $.ajax({
                url: '/Admin/RestoreEvent/' + eventId,
                type: 'GET',
                success: function (response) {
                    $('#EventsTable').DataTable().ajax.reload();
                },
                error: function (xhr) {
                    alert("Error restoring event.");
                }
            });
        }
    });

} )