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
            "url": "/api/GetArchiveEvents",
            "type": "POST",
            "dataType": "json"
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
            { "data": "title", "name": "EventTitle", "autowidth": true },
            { "data": "start_Date", "name": "EventFrom", "autowidth": true },
            { "data": "end_Date", "name": "EventTo", "autowidth": true },
            { "data": "venue", "name": "EventVenue", "autowidth": true },
            { "data": "created_On", "name": "CreatedOn", "autowidth": true },
            { "data": "created_By", "name": "Created_By", "autowidth": true, orderable: false },
            { "data": "status", "name": "Status", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {


                    //check if Role = Agent Don't show Action btn
                    if (window.isAgent === true || window.isAgent === "true") {
                        return '<span>No Action</span>';
                    }

                    if (window.isSupervisor === true || window.isSupervisor === "true" || window.isAdmin === true || window.isAdmin === "true") {
                        return ` 
                            <div class="btn-group">
                     <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                      Action
                      </button>
                                          <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">
                            <a href="/admin/viewevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-eye" style="color:cornflowerblue"></i>&nbsp; View Event</a> 
             
                                                                  <a href="/admin/GetEventAssignedOperator/${row.id}" disabled="disabled" class="dropdown-item"><i class="nav-icon fas fa-user" style="color:cornflowerblue"></i>&nbsp; Assign Operator</a>
                             </div>
                              
                               </div>`
                    } else {
                        return ` 
                            <div class="btn-group">
                     <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                      Action
                      </button>
                                          <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">
                            <a href="/admin/viewevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-eye" style="color:cornflowerblue"></i>&nbsp; View Event</a> 
             

                             </div>
                              
                               </div>`
                    }
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
})