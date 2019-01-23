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
        if ($("#create").hasClass("folder"))
            $("#create").removeClass("folder");
        $("#creator").toggle();
    });

    $(".newfolder").on("click", function () {
        $("#creator .tt").text("New folder");
        $("#creator .stt").text("Name");
        $("#creator select").css("visibility", "hidden");
        if (!$("#create").hasClass("folder"))
            $("#create").addClass("folder");
        $("#creator").toggle();
    });

    $("#create").on("click", function () {
        var v = $("#creator input").val();
        var v1 = $("#creator option:selected").text();
        if (!$(this).hasClass("folder")) {
            try {
                $("#sb .stt:contains('" + v1 + "')").eq(0).after("<div class='pc'>" + v + "</div>");
                console.log(v1);
                $.post('/Note/CreateNote',
                    {
                        title: v,
                        name: v1
                    }, function () { });
                
            } catch (e) {
                console.log("[!!!] note creation");
            }
        } else {
            try {
                $("#sb .tt").after("<div class='stt'>" + v + "</div>");
                $.post('/Note/CreateFolder',
                    {
                        name: v
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