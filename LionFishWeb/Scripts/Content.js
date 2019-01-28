$(document).ready(function () {

    $("#sectab").hide();
    $("#nottab").hide();
    $("#dgr").hide();

    $(".pc").on("click", function () {
        var re;
        if (!$(".editable").length == 0) {
            re = confirm("You have unsaved changes. Continue?");
        }
        if ($(".editable").length == 0 || re) {
            var opt = $(this).text();
            switch (opt) {
                case "Security":
                    $("#gentab").hide();
                    $("#sectab").show();
                    $("#nottab").hide();
                    break;
                case "Notifications":
                    $("#gentab").hide();
                    $("#sectab").hide();
                    $("#nottab").show();
                    break;
                default:
                    $("#gentab").show();
                    $("#sectab").hide();
                    $("#nottab").hide();
            }
            $(".editable").removeClass("editable");
            $("input").removeProp("disabled");
            return;
        }
    });

    $(".e").on("click", function () {
        var nx = $(this).next();
        var ds = nx.prop("disabled");
        nx.prop("disabled", !ds).focus();
        nx.toggleClass("editable");
        $(this).text(ds ? 'Save' : 'Edit');

        if ($(this).text() != "save") {
            $.post('/App/UpdateProfileInfo',
                {
                    name: nx.attr("id"),
                    info: nx.val()
                }, function () { });
        }
    });

    $("#pshows").on("click", function () {
        var private = $(this).prev().prop("checked");
        $.post('/App/UpdatePrivacy', { private: private }, function () {});
    });

    $("#upimg").on("change", function (e) {
        var $files = $(this).get(0).files;
        if($files[0].size > $(this).data("max-size") * 1024) {
            return false;
        }
        console.log("Uploading file to Imgur..");
        
        var apiUrl = 'https://api.imgur.com/3/image';
        var apiKey = 'e76d8888df0f805';

        var settings = {
            async: false,
            crossDomain: true,
            processData: false,
            contentType: false,
            type: 'POST',
            url: apiUrl,
            headers: {
                Authorization: 'Client-ID ' + apiKey,
                Accept: 'application/json'
            },
            mimeType: 'multipart/form-data'
        };

        var formData = new FormData();
        formData.append("image", $files[0]);
        settings.data = formData;
        
        $.ajax(settings).done(function (response) {
            var resp = JSON.parse(response);
            $("#prv img").attr("src", resp.data.link);
            $.post('/App/UpdateProfileImg', { profileimg: resp.data.link }, function () { });
        });
     });

    $("#chpass").on("click", function () {
        if ($(this).text() == "show password settings") {
            if (!$("#cchpass").length) {
                $(this).after("<span id='cchpass' role='button' style='display:inline-block;margin:5px 0 0 8px;height:15px;color:#000;'>Confirm</span>");
            }
        } else {
            $(this).text("show password settings");
            $("#cchpass").remove();
            $("#dgr").hide();
        }
    });

    $(document).on("click", "#cchpass", function () {
        $("#dgr").show();
        $("#chpass").text("hide password settings");
        $(this).hide();
    });

    function intToZXC(i) {
        var res = [];
        switch (i) {
            case 0:
                res[0] = "extremely weak. will be rejected";
                res[1] = "black";
                break;
            case 1:
                res[0] = "very weak. will be rejected";
                res[1] = "red";
                break;
            case 2:
                res[0] = "weak";
                res[1] = "orange";
                break;
            case 3:
                res[0] = "moderate";
                res[1] = "yellowgreen";
                break;
            case 4:
                res[0] = "strong";
                res[1] = "limegreen";
                break;
            default:
                res[0] = "extremely weak";
                res[1] = "black";
        }
        return res;
    }

    $("input[type='password']").on("keyup", function () {
        var v = $(this).val();
        if (v.length > 0) {
            $(this).next().removeClass("hide");
        } else {
            $(this).next().addClass("hide");
        }
    });

    $(".show-password").on("click", function () {
        var huh = $(this).text();
        var x = $(this).prev();
        if (x.attr("type") === "password") {
            x.prop("type", "text");
            $(this).text("hide");
        } else {
            x.prop("type", "password");
            $(this).text("show");
        }
    });

    $("#newp").on("keyup", function () {
        var inp = $(this).val();
        var x = zxcvbn(inp);
        var uhh = intToZXC(x.score);
        $("#strength-color").css("background-color", uhh[1]);
        $("#strength-title").text(uhh[0]);
        $("#strength-desc").text("This will be cracked in " + x.crack_times_display.offline_slow_hashing_1e4_per_second + ". " + x.feedback.warning);
    });

    tippy('#newp', {
        content: "<div id='tip-strength'><p><div id='strength-color'></div><span id='strength-title'>extremely weak</span><br><span id='strength-desc'></span></p></div>",
        delay: 100,
        placement: 'top-start',
        interactive: false,
        arrow: false,
        trigger: 'click',
        size: 'large',
        theme: 'light',
        duration: 100,
        animation: 'fade'
    });

    $("#spass").on("click", function () {
        var o = $("#currentp").val();
        var n = $("#newp").val();
        $.post('/App/UpdatePasswordAsync', {
            oldPassword: o,
            newPassword: n
        }, function () { });
    });

});