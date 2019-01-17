$(function () {

    $("#newnf").on("click", function () {
        $("#newnfc").toggle();
    });

    $("#newnote").on("click", function () {
        // logic
    });

    $("#newfolder").on("click", function () {
        // remove hardcoding
        $("#creator").toggle();
    });

    $(".cancel").on("click", function () {
        $("#creator").toggle();
    });

});