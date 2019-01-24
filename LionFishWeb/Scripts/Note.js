$(function () {
    $("#swap1").hide();
    $("#swap2").show();

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
                $("#creator select").append("<option>" + v + "</option>");
                $.post('/Note/CreateFolder',
                    {
                        name: v
                    }, function () { });
            } catch (e) {
                console.log("[!!!] folder creation");
            }
        }
        $("#creator input").val("");
    });

    $(".cancel").on("click", function () {
        $("#creator").toggle();
        $("#creator input").val("");
    });

    $("#sb").on("click", ".pc", function () {
        $.ajax({
            type: "GET",
            url: "/Note/GetNoteDetails",
            data: { ID: $(this).attr('id') },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            success: function (result) {
                $("#swap1").hide();
                $("#swap2").show();
                $("#rt .tt input").val(result.Title);
                if(result.Content != null)
                    quill.setText(result.Content);
                else
                    quill.setText("");
                console.log(result);
            },
            error: function (response) { }
        });
    });

    $(window).bind('keydown', function (event) {
        if (event.ctrlKey || event.metaKey) {
            switch (String.fromCharCode(event.which).toLowerCase()) {
                case 's':
                    event.preventDefault();
                    console.log('ctrl + s pressed. saving current note.');
                    break;
            }
        }
    });
});