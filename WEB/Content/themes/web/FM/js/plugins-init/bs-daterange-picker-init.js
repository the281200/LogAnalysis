(function($) {
    "use strict"

    // Daterange picker
    //$('.input-daterange-datepicker').daterangepicker({
        //buttonClasses: ['btn', 'btn-sm'],
        //applyClass: 'btn-danger',
        //cancelClass: 'btn-inverse'
    //});
    //$('.input-daterange-timepicker').daterangepicker({
    //    timePicker: true,
    //    format: 'MM/DD/YYYY h:mm A',
    //    timePickerIncrement: 30,
    //    timePicker12Hour: true,
    //    timePickerSeconds: false,
    //    buttonClasses: ['btn', 'btn-sm'],
    //    applyClass: 'btn-danger',
    //    cancelClass: 'btn-inverse'
    //});
    $('.input-limit-datepicker').daterangepicker({
        format: 'MM/DD/YYYY',
        minDate: '09/01/1999',
        maxDate: '10/25/1999',
        buttonClasses: ['btn', 'btn-sm'],
        applyClass: 'btn-danger',
        cancelClass: 'btn-inverse',
        dateLimit: {
            days: 6
        }
    });
})(jQuery);