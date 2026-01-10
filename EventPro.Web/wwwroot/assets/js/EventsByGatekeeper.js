$(document).ready(function () {
    
    if ($('#Assigned').val() == 'false') {
        $("#GKId")[0].selectedIndex = 0;
        $("#GKId").change();
        document.getElementById("GKId").disabled = true;
    }
    else {
        document.getElementById("GKId").disabled = false;
    }
    $('#Assigned').change(function () {
        if (this.value == 'false') {
            $("#GKId")[0].selectedIndex = 0;
            $("#GKId").change();
            document.getElementById("GKId").disabled = true;
        }
        else {
            document.getElementById("GKId").disabled = false;
        }
    });
    
        var dataTable = $("#ResultTable").DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "language": {
                "emptyTable": "No Data found"
            },
            "ajax": {
                "url": "/Admin/FilterEventsByGK",
                "type": "POST",
                "datatype": "json",
                "data": function (d) {

                    d.eventTitle = $('#EventTitle').val();
                    d.eventId = $('#EventId').val();
                    d.address = $('#Address').val();
                    d.startFrom = $('#startFrom').val();
                    d.startTo = $('#startTo').val();

                    d.type = $('#type').val();
                    d.assigned = $('#Assigned').val();
                    d.gKId = $('#GKId').val();                    
                }
            },
            order: [[0, 'desc']],
            "columnDefs": [{
                "targets": [0],
                "visible": false
            }],
            "columns": [
                
                { "data": "id", "name": "Id", "autoWidth": true },
                { "data": "id", "name": "Id", "autoWidth": true },
                {
                    "data": "eventTitle", "name": "EventTitle", "autoWidth": true,
                    "render": function (data, type, row) {
                        debugger;
                        if (row.icon) {

                            return ` <div>     <a href="admin/viewevent/${row.id}">

                                                        <img src="~/Upload/card/${row.icon}" class="grid-icon" />${row.eventTitle}

                                                </a> <div/>`
                        }
                        else {
                            ` <div>     <a href="/viewevent/${row.id}">
                                               <img src="~/Upload/card/default.png" class="grid-icon" /> ${row.eventTitle}
                                                </a> <div/>`
                        }
                    }
                },
                { "data": "linkedTo", "name": "LinkedEvent", "autoWidth": true },
                {
                    "data": "eventFrom", "name": "EventFrom", "autoWidth": true,
                    "render": function (data, type, row) {
                        return moment(row.eventFrom).format('ll')
                    }
                },

                {
                    "data": "eventTo", "name": "EventTo", "autoWidth": true,
                    "render": function (data, type, row) {
                        return moment(row.eventTo).format('ll')
                    }
                },
                { "data": "eventVenue", "name": "EventVenue", "autoWidth": true },
                { "data": "location", "name": "Location", "autoWidth": true },                
                {
                    "data": "gatekeeperIds", "name": "GatekeeperIds", "autoWidth": true, "orderable": "false",
                    "render": function (data, type, row) {
                        return `<div class="font-weight-bold text-primary">${row.gatekeeperIds}</div> `
                    }
                },                
                {
                    "data": "gatekeeperNames", "name": "GatekeeperNames", "autoWidth": true,
                    "render": function (data, type, row) {
                        return `<div class="font-weight-bold text-primary">
                          <a href="/admin/viewevent/${row.id}">
                            ${row.gatekeeperNames}
                                  </a> <div/>`
                    }
                }               
            ]
        });

        $('#RefreshPage').click(function () {
            window.location.reload();
        });
        $('#SearchBtn').click(function () {
            dataTable.draw();
        });
});