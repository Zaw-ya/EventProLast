$(document).ready(function () {
    console.log(@ViewBag.EventId);
    $('#GuestsTable').DataTable({
        "bStateSave": true,
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
            "url": "/admin/GetGuests?id=" + @ViewBag.EventId,
            "type": "POST",
            "dataType": "json"
        },
        "columnDefs": [{
            "targets": [0],
            "visible": false,
            "searchable": false
        }],
        "columns": [
            {
                "render": function (data, type, row) {
                    return `<a href="/admin/StatusRefreshIndividually/${row.id}?eventid=${@ViewBag.EventId}" class="rotate-icon">
                                                                                <i class="fas fa-sync-alt" title="تحديث الحالة" style="color: darkgreen;"></i>
                                                                            </a>`}, "autowidth": true, "orderable": false, "data": null
            },
            {
                "render": function (data, type, row) {
                    return `<a onclick="GuestStatusRefresh(${row.id},${@ViewBag.EventId})" class="rotate-icon" style="cursor: pointer;">
                                                                                        <i class="fas fa-sync-alt" title="تحديث الحالة" style="color: darkgreen;"></i>
                                                                                    </a>`}, "autowidth": true, "orderable": false, "data": null
            },
            { "data": "id", "name": "Id", "autowidth": true, "orderable": false },
            { "data": "eventId", "name": "EventId", "autowidth": true, "orderable": false },
            { "data": "name", "name": "Name", "autowidth": true, "orderable": false },
            {
                "render": function (data, type, row) {
                    return `<a href="tel:${row.phoneNumber}" if(${row.isValidPhoneNumber}{
                                    style="color:green;"} else{
                                            style="color:red;"})>${row.phoneNumber}</a>`
                }, "autowidth": true, "data": null, "orderable": false
            },
            { "data": "scannedInfo", "name": "ScannedInfo", "autowidth": true, "orderable": false },

        ]

    });
});