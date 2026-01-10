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
            "url": "/api/GetEvents",
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
            { "data": "created_On", "name": "CreatedOn", "autowidth": true },
            { "data": "created_By", "name": "Created_By", "autowidth": true, orderable: false },
            { "data": "status", "name": "Status", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {

                    switch (row.status) {
                        case "past":
                            return ` 
                                     <div class="btn-group">
                                          <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                          Action
                                          </button>

                                          <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">

                                                 <a href="/admin/viewevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-eye" style="color:cornflowerblue"></i>&nbsp;
                                                 View Event
                                                 </a> 

                                                <a href="#" class="dropdown-item text-danger delete-event" data-id="${row.id}">
                                                    <i class="nav-icon fas fa-trash-alt" style="color:red"></i>&nbsp; Delete Event
                                                </a>

                                          </div>
                                      </div>
                                   `
                             break;
                        case "in-progress":
                            return `
                                    <div class="btn-group">

                                             <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                              Action
                                             </button>

                                             <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">

                                                 <a href="/admin/viewevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-eye" style="color:cornflowerblue"></i>&nbsp;
                                                 View Event
                                                 </a>

                                                 <a href="#" class="dropdown-item text-danger delete-event" data-id="${row.id}">
                                                    <i class="nav-icon fas fa-trash-alt" style="color:red"></i>&nbsp; Delete Event
                                                 </a>
                                                  <div class="dropdown-divider"></div>

                                                  <a href="/admin/guests/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-qrcode" style="color:cornflowerblue"></i>&nbsp;
                                                  Event Guest
                                                  </a>

                                                  <a href="#" disabled="disabled" class="dropdown-item"><i class="nav-icon fas fa-asterisk" style="color:cornflowerblue"></i>&nbsp;
                                                  QR Settings
                                                  </a>
                              
                                                  <div class="dropdown-divider"></div>
                                                  <a href="/admin/assigngatekeeper/${row.id}" disabled="disabled" class="dropdown-item"><i class="nav-icon fas fa-user" style="color:cornflowerblue"></i>&nbsp;
                                                  Assign Gatekeeper
                                                  </a>
                                      <div class="dropdown-divider"></div>
                                      <a href="/admin/GetEventAssignedOperator/${row.id}" disabled="disabled" class="dropdown-item"><i class="nav-icon fas fa-user" style="color:cornflowerblue"></i>&nbsp; Assign Operator</a>
                                      <div class="dropdown-divider"></div>
                                    
                                                   <a href="/admin/editevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-archive" style="color:cornflowerblue"></i>&nbsp;
                                                   Archive
                                                   </a>

                                            </div>

                                       </div>
                                    `
                            break;

                        case "upcoming":
                            return `
                             <div class="btn-group">
                     <button type="button" class="btn btn-warning btn-sm dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                      Action
                      </button>
                                                         <div class="dropdown-menu" style="font-size: small;z-index:5; margin-left:-80px;">
                            <a href="/admin/editevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-edit" style="color:cornflowerblue"></i>&nbsp; Edit Event</a>
                                 <a href="#" class="dropdown-item text-danger delete-event" data-id="${row.id}">
                                                    <i class="nav-icon fas fa-trash-alt" style="color:red"></i>&nbsp; Delete Event
                                                 </a>
                                                  <div class="dropdown-divider"></div>
                                <a href="/admin/viewevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-eye" style="color:cornflowerblue"></i>&nbsp; View Event</a>
                                                                            
                                <div class="dropdown-divider"></div>
                                <a href="/admin/guests/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-qrcode" style="color:cornflowerblue"></i>&nbsp; Event Guest</a>
                                <a href="/admin/QRSettings/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-asterisk" style="color:cornflowerblue"></i>&nbsp; QR Settings</a>
                                                          <div class="dropdown-divider"></div>
                                                                        <a href="/admin/assigngatekeeper/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-user" style="color:cornflowerblue"></i>&nbsp; Assign Gatekeeper</a>
                                      <div class="dropdown-divider"></div>
                                      <a href="/admin/GetEventAssignedOperator/${row.id}" disabled="disabled" class="dropdown-item"><i class="nav-icon fas fa-user" style="color:cornflowerblue"></i>&nbsp; Assign Operator</a>
                                      <div class="dropdown-divider"></div>
  
                                <a href="/admin/editevent/${row.id}" class="dropdown-item"><i class="nav-icon fas fa-archive" style="color:cornflowerblue"></i>&nbsp; Archive</a>
                                                                    </div>
                               </div>`

                            break;

                           
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

    $(document).on("click", ".delete-event", function (e) {
        e.preventDefault();
        var eventId = $(this).data("id");

        if (confirm("Are you sure you want to delete this event?")) {
            $.ajax({
                url: '/Admin/DeleteEvent/' + eventId,
                type: 'GET',
                success: function (response) {
                    //alert(response.message);
                    $('#EventsTable').DataTable().ajax.reload();
                },
                error: function (xhr) {
                    alert("Error deleting event.");
                }
            });
        }
    });
} )