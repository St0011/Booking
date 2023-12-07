$(function () {
    $('[data-toggle="tooltip1"]').tooltip();

});
$(function () {
    $('[data-toggle="tooltip2"]').tooltip();
});

$(function () {
    $('[data-toggle="tooltip3"]').tooltip();
});


$(document).ready(function () {
    var currentDate = new Date().toISOString().split('T')[0];
    $('#selectedDateInput').attr('min', currentDate);
});


