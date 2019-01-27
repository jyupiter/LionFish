$(function () {
    $("#swap1").hide();
    $("#swap2").show();

    var quill = new Quill('#editor', {
        theme: 'snow'
    });

    $('<div id="editor">').data('quill', quill);

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
                $.post('/Note/CreateNote',
                    {
                        title: v,
                        name: v1
                    }, function () { });
                $.ajax({
                    type: "GET",
                    url: "/Note/GetNoteDetails",
                    data: { ID: $(this).attr('id') },
                    contentType: "application/json;charset=utf-8",
                    dataType: "json",
                    success: function (result) {
                        $("#sb .stt:contains('" + v1 + "')").eq(0).after("<div class='pc' id='" + result.ID + "'>" + v + "</div>");
                    },
                    error: function (response) { }
                });
            } catch (e) {
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
        setQuill(result.Content);
        $("#selected").removeClass();
        $("#selected").addClass(result.ID);
    }

    function setQuill(content) {
        quill.setText('');
        if (content != "")
            quill.setContents($.parseJSON(content));
    }

    $("#sb").on("click", ".pc", function (passed) {
        $.ajax({
            type: "GET",
            url: "/Note/GetNoteDetails",
            data: { ID: $(this).attr('id') },
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            success: function (result) {
                $("#ntitle").val($(passed.target).text());
                changeSelectedNote(result);

                $.ajax({
                    type: "GET",
                    url: "/Note/GetFolderName",
                    data: { ID: result.FolderID },
                    contentType: "application/json;charset=utf-8",
                    dataType: "json",
                    success: function (result1) {
                        $("#note-folder").text(result1);
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
                            $("#note-event").text(result2);
                            $("#note-event").removeClass();
                            $("#note-event").addClass(result.EventID);
                        },
                        error: function (response) { }
                    });
                } else {
                    $("#note-event").text("Not event linked");
                }
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
        $("#save-out").text("Saved");
    }

    function autosave() {
        if ($("#editor").attr("data-dirty") == true) {
            var nid = $("#selected").attr("class");
            save(nid);
        }
    }

    $("#ntitle").on("keyup", function () {
        $("#editor").attr("data-dirty", "true");
        $("#save-out").text("Unsaved changes");
    });

    quill.on('text-change', function () {
        $("#editor").attr("data-dirty", "true");
        $("#save-out").text("Unsaved changes");
    });

    $("#save").on("click", function () {
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
                        if (nid != "empty")
                            save(nid);
                    break;
            }
        }
    });

    setInterval(autosave, 2500);

    var setSelectedNote = function (id) {
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