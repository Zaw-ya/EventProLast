$(document).ready(function () {
    var dataTable = $("#ResultTable").DataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        "language": {
            "emptyTable": "No Data found"
        },
        "ajax": {
            "url": "/Admin/FilterTotalGuests",
            "type": "POST",
            "datatype": "json",
            "data": function (d) {
                d.Address = $('#Address').val();
                d.startFrom = $('#startFrom').val();
                d.startTo = $('#startTo').val();
                d.type = $('#type').val();
                d.eventTitle = $('#EventTitle').val();
                d.eventId = $('#EventId').val();
            }
        },
        order: [[0, 'desc']],
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            { "data": "id", "name": "Id", "autoWidth": true },
            { "data": "id", "name": "Id", "autoWidth": true },
            {
                "data": "eventTitle", "name": "EventTitle", "autoWidth": true,
                "render": function (data, type, row) {

                        return ` <div>     <a href="/admin/viewevent/${row.id}">
                                                 
                                                        ${row.eventTitle}                                                   
                                               
                                                </a> <div/>`

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
                "data": "totalGuests", "name": "TotalGuests", "autoWidth": true,
                "render": function (data, type, row) {
                    
                    return ` <div class="font-weight-bold text-primary">   
                             <a href="/admin/guests/${row.id}">
                                               ${row.totalGuests}                                              
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