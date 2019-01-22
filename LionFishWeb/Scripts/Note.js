$(function () {

    var quill = new Quill('#editor', {
        theme: 'snow'
    });

    $("#newnf").on("click", function () {
        $("#newnfc").toggle();
    });

    $(".newnote").on("click", function () {
        $("#creator .tt").text("New note");
        $("#creator .stt").text("Title");
        $("#creator select").css("visibility", "visible");
        $("#creator").toggle();
    });

    $(".newfolder").on("click", function () {
        $("#creator .tt").text("New folder");
        $("#creator .stt").text("Name");
        $("#creator select").css("visibility", "hidden");
        $("#create").toggleClass("folder");
        $("#creator").toggle();
    });

    $("#create").on("click", function () {
        var v = $("#creator input").val();
        var v1 = $("#creator option:selected").text();
        if (!$(this).hasClass("folder")) {
            try {
                if (v1 == "Folder (Optional)") {
                    v1 = "";
                    $("#sb .tt").after("<div class='pc'>" + v + "</div>");
                } else
                    $("#sb .tt:contains('" + v1 + "')").after("<div class='pc'>" + v + "</div>");
                $.post('/Note/CreateNote',
                    {
                        title: v,
                        folder: v1
                    }, function () { });
            } catch (e) {
                console.log("[!!!] note creation");
            }
        } else {
            try {
                $("#sb .tt").after("<div class='stt'>" + v + "</div>");
                $.post('/Note/CreateFolder',
                    {
                        name: v,
                    }, function () { });
            } catch (e) {
                console.log("[!!!] folder creation");
            }
        }
    });

    $(".cancel").on("click", function () {
        $("#creator").toggle();
    });

});