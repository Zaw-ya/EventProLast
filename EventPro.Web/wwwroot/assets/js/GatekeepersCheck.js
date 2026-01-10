$(document).ready(function () {  
    
    var dataTable = $("#ResultTable").DataTable({

            "processing": true,
            "serverSide": true,
            "filter": true,
            "language": {
                "emptyTable": "No Data found",
                /*processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'*/
            },
            "ajax": {
                "url": "/Admin/FilterGKsCheck",
                "type": "POST",
                "datatype": "json",
                "data": function (d) {

                    d.eventTitle = $('#EventTitle').val();
                    d.eventId = $('#EventId').val();
                    d.address = $('#Address').val();
                    d.startFrom = $('#startFrom').val();
                    d.startTo = $('#startTo').val();

                    d.type = $('#type').val();
                    
                    d.gKId = $('#GKId').val(); 
                    d.checktype = $('#checktype').val();
                    
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
                        
                        if (row.icon) {

                            return ` <div>     <a href="/admin/viewevent/${row.id}">

                                                        <img src="/Upload/card/${row.icon}" class="grid-icon" />${row.eventTitle}

                                                </a> <div/>`
                        }
                        else {
                            ` <div>     <a href="/viewevent/${row.id}">
                                               <img src="/Upload/card/default.png" class="grid-icon" /> ${row.eventTitle}
                                                </a> <div/>`
                        }
                    }
                },
                { "data": "linkedTo", "name": "LinkedEvent", "autoWidth": true, "orderable": false },
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
                { "data": "eventVenue", "name": "EventVenue", "autoWidth": true, "orderable": false },
                { "data": "location", "name": "Location", "autoWidth": true, "orderable": false },  


                { "data": "gatekeeperId", "name": "GatekeeperId", "autoWidth": true, "orderable": false },            
                { "data": "gatekeeperName", "name": "GatekeeperName", "autoWidth": true, "orderable": false },            
                {
                    "data": "checkType", "name": "CheckType", "autoWidth": true,
                    "render": function (data, type, row) {
                        debugger;
                        if (row.checkType == "In") {
                            return ` <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-arrow-right-circle-fill" viewBox="0 0 16 16">
                                <path d="M8 0a8 8 0 1 1 0 16A8 8 0 0 1 8 0M4.5 7.5a.5.5 0 0 0 0 1h5.793l-2.147 2.146a.5.5 0 0 0 .708.708l3-3a.5.5 0 0 0 0-.708l-3-3a.5.5 0 1 0-.708.708L10.293 7.5z" />
                            </svg> ${row.checkType}`
                        }
                        else if (row.checkType == "Out") {
                            return ` <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-arrow-left-circle" viewBox="0 0 16 16">
                                <path fill-rule="evenodd" d="M1 8a7 7 0 1 0 14 0A7 7 0 0 0 1 8m15 0A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-4.5-.5a.5.5 0 0 1 0 1H5.707l2.147 2.146a.5.5 0 0 1-.708.708l-3-3a.5.5 0 0 1 0-.708l3-3a.5.5 0 1 1 .708.708L5.707 7.5z" />
                            </svg> ${row.checkType}`
                        }
                        else return row.checkType 
                    }
                },    

                { "data": "logDate", "name": "LogDate", "autoWidth": true },      
                {
                    "data": "latitude", "name": "latitude", "autoWidth": true, "orderable": false ,
                    "render": function (data, type, row) {
                        return `<a href="http://maps.google.com/?q=${row.latitude},${row.longitude}" target="_blank">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-geo-alt-fill" viewBox="0 0 16 16">
                                <path d="M8 16s6-5.686 6-10A6 6 0 0 0 2 6c0 4.314 6 10 6 10m0-7a3 3 0 1 1 0-6 3 3 0 0 1 0 6" />
                            </svg>
                        </a>`
                    }

                },     
                      
            ]
        });

        $('#RefreshPage').click(function () {
            window.location.reload();
        });
        $('#SearchBtn').click(function () {
            dataTable.draw();
        });
});