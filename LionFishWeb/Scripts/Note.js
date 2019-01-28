$(function () {
    $("#swap1").show();
    $("#swap2").hide();

    var quill = new Quill('#editor', {
        placeholder: 'Type anything...',
        theme: 'snow'
    });
    
    $("#swap2").data('quill', quill);

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
        $("#selected").removeClass();
        $("#selected").addClass(result.ID);
    }

    function decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    }

    $("#swap2").on("change", "#editor", function () {
        $("#editor").attr("data-dirty", "true");
        $("#save-out").text("Unsaved changes");
    });

    $("#sb").on("click", ".pc", function (passed) {
        $("#ned").remove();
        $("#swap2 .stt").after("<div id='ned' class='bc'><div id='editor' data-dirty='false'></div></div>");
        var quill = new Quill('#editor', {
            placeholder: 'Type anything...',
            theme: 'snow'
        });
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
                if (result.Content != "") {
                    var rjson = decodeHtml(result.Content);
                    var bjson = rjson.replace(/\\\"/g, '\"');
                    var cjson = bjson.substring(1, bjson.length - 1);
                    quill.setContents($.parseJSON(cjson));
                }
                $("#note-event").removeClass();
                $("#note-event").text("No event linked");
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
                }
            },
            error: function (response) { }
        });
    });

    function save(nid) {
        console.log($("#selected").attr("data-content"));
        $.post('/Note/UpdateNote',
            {
                ID: nid,
                title: $("#ntitle").val(),
                content: JSON.stringify($("#selected").attr("data-content"))
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

    $("#save").on("click", function () {
        var nid = $("#selected").attr("class");
        if (nid != null)
            if (nid != "empty")
                save(nid);
    });

    $("#swap2").on("keyup", "#editor", function (event) {
        $("#selected").attr("data-content", JSON.stringify($(this)[0].__quill.editor.delta));
    });

    $(window).bind('keydown', function (event) {
        if (event.ctrlKey || event.metaKey) {
            switch (String.fromCharCode(event.which).toLowerCase()) {
                case 's':
                    event.preventDefault();
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

	$("#note-event").on("click", function () {
		console.log($(this).attr("id"));
        $.post('/Calendar/SetEventByID',
            {
                ID: $(this).attr("class")
            }, function () { });
        window.location.replace("/Calendar/Calendar");
    });

    $("#delete").on("click", function () {
        $("#swap1").show();
        $("#swap2").hide();
        var sel = $("#selected").attr("class");
        $.post('/Note/DeleteNote',
            {
                ID: sel
            }, function () { });
        $("#selected").attr("class", "empty");
        window.location.reload();
    });
});