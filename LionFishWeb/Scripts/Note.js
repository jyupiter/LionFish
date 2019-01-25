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

    function changeSelectedNote(result) {
        $("#swap1").hide();
        $("#swap2").show();
        $("#ntitle").val(result.Title);
        $("#selected").removeClass();
        $("#selected").addClass(result.ID);
    }

    $("#sb").on("click", ".pc", function () {
        $.ajax({
            type: "GET",
            url: "/Note/GetNoteDetails",
            data: { ID: $(this).attr('id') },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            success: function (result) {
                changeSelectedNote(result);

                $.ajax({
                    type: "GET",
                    url: "/Note/GetFolderName",
                    data: { ID: result.FolderID },
                    contentType: "application/json;charset=utf-8",
                    dataType: "json",
                    success: function (result1) {
                        $("#note-folder").text(result1.Name);
                    },
                    error: function (response) { }
                });

                if (result.EventID != null) {
                    $.ajax({
                        type: "GET",
                        url: "/Note/GetEventTitle",
                        data: { ID: result.EventID },
                        contentType: "application/json;charset=utf-8",
                        dataType: "json",
                        success: function (result2) {
                            $("#note-event").text(result2.Title);
                        },
                        error: function (response) { }
                    });
                } else {
                    $("#note-event").text("Not linked from an event");
                }
                
                if(result.Content != null)
                    quill.setText(result.Content);
                else
                    quill.setText("");
                console.log(result);
            },
            error: function (response) { }
        });
    });

    function save(nid) {
        $.post('/Note/UpdateNote',
            {
                ID: nid,
                title: $("#ntitle").val(),
                content: JSON.stringify(quill.getContents())
            }, function () { });
    }

    function autosave() {
        if ($("#editor").attr("data-dirty") == true) {
            var nid = $("#selected").attr("class");
            save(nid);
        }
    }

    $("#ntitle").on("keyup", function () {
        $("#editor").attr("data-dirty", "true");
    });

    quill.on('text-change', function () {
        $("#editor").attr("data-dirty", "true");
    });

    $("#save-out").on("click", function () {
        var nid = $("#selected").attr("class");
        if (nid != null)
            if (nid != "empty")
                save(nid);
    });

    $(window).bind('keydown', function (event) {
        if (event.ctrlKey || event.metaKey) {
            switch (String.fromCharCode(event.which).toLowerCase()) {
                case 's':
                    event.preventDefault();
                    console.log("ctrl + s pressed. saving current note.");
                    var nid = $("#selected").attr("class");
                    if (nid != null)
                        if(nid != "empty")
                            save(nid);
                    break;
            }
        }
    });

    setInterval(autosave, 2500);

    function setSelectedNote(id) {
        $.ajax({
            type: "GET",
            url: "/Note/GetNoteDetails",
            data: { ID: id },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            success: function (result) {
                changeSelectedNote(result);
            },
            error: function (response) { }
        });
    }
});