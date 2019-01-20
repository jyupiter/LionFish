$(function () {

    $("#newnf").on("click", function () {
        $("#newnfc").toggle();
    });

    $("#newnote").on("click", function () {
        $("#creator .tt").text("New note");
        $("#creator .stt").text("Title");
        $("#creator").toggle();
    });

    $("#newfolder").on("click", function () {
        $("#creator .tt").text("New folder");
        $("#creator .stt").text("Name");
        $("#create").toggleClass("folder");
        $("#creator").toggle();
    });

    $("#create").on("click", function () {
        if (!$(this).hasClass("folder")) {
            // post
        } else {
            // post
        }
    });

    $(".cancel").on("click", function () {
        $("#creator").toggle();
    });

});