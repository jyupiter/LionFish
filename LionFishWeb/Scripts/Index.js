$(function () {

    $(".input-email").on("keyup", function () {
        $('span.error-invalid-email').hide();
        var v = $(this).val();
        var r = /[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?/;
        if (!r.test(v) && v.length > 0) {
            $(this).after('<span class="error error-invalid-email">Invalid email!</span>');
        }
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

    $("#input-sign-password").on("keyup", function () {
        var inp = $("#input-sign-password").val();
        var x = zxcvbn(inp);
        var uhh = intToZXC(x.score);
        $("#strength-color").css("background-color", uhh[1]);
        $("#strength-title").text(uhh[0]);
        $("#strength-desc").text("This will be cracked in " + x.crack_times_display.offline_slow_hashing_1e4_per_second + ". " + x.feedback.warning);
    });

    $(".input-password").on("keyup", function () {
        var v = $(this).val();
        if (v.length > 0) {
            $(this).next().removeClass("hide");
        } else {
            $(this).next().addClass("hide");
        }
    });

    tippy('#input-sign-password', {
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

});