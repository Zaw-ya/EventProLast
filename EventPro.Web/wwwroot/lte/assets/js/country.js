$(document).ready(function () {  

    $('.js-delete').on('click', function () {
        var btn = $(this);
        const swalWithBootstrapButtons = Swal.mixin({
            customClass: {
                confirmButton: "btn btn-danger mx-2",
                cancelButton: "btn btn-light"
            },
            buttonsStyling: false
        });
        swalWithBootstrapButtons.fire({
            title: `Are you sure to delete "${btn.data('name')}" ?`,
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "No, cancel!",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Country/Delete/${btn.data('id')}`,
                    method: 'Delete',
                    success: function () {
                        Swal.fire(
                            'Deleted!',
                            `"${btn.data('name')}" country has been deleted.`,
                            'success'
                        )
                        btn.parents('tr').fadeOut();
                    },
                    error: function (response) {
                        var message = 'Something went wrong.';
                        debugger;  
                        if (response.responseText !== "") {                           
                            message = jQuery.parseJSON(response.responseText).responseText;
                            debugger;  
                        }                    
                        Swal.fire(
                            'Oooops...!',                           
                            message ,                           
                            'error'
                        )
                    }
                })
            } 
        });  
    })
})