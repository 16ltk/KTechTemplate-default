function DoDatePicker(controlname) {
    $('#' + $.escapeSelector(controlname)).datepicker({
        uiLibrary: 'bootstrap4',
        iconsLibrary: 'fontawesome',
        format: 'yyyy-mm-dd',
    });

}