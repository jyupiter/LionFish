$(function () {


    $.connection.hub.url = "https://localhost:44393/signalr";
    $.connection.hub.start()
        .done(function () {
          //  $.connection.myHub.server.ShowGroups();
            //console.log("m8");
            // var roomSelected = $("input[name='chatRoom']:checked").val();
            //  $.connection.myHub.server.joinRoom(roomSelected);


        })
        .fail(function () {
            //alert("oi m8");
        })

    $("#genData").on("click", function () {
        console.log("awewae");
        $.connection.myHub.server.showGroups();

        })
    $.connection.myHub.client.showGroups = function () {

        console.log("test");
    }
    //put this into server  
    $(".roomSelect").change(function () {
        console.log("a");
        removeText();
        loadText();





    })

    function removeText() {

        console.log("removeit dick");
        $("#NewMessage").text("");

    }



    $.connection.myHub.client.changeRoom = function (returnJson) {

        var unJs = JSON.parse(returnJson);
        console.log(unJs);
        for (var i = 0; i < unJs.messageList.length; i++) {
            $("#NewMessage").append(unJs.messageList[i]);

        }

    }

    $('#sendText').on("click", function() {

        if ($('#area').val() === '' || $('#area').val() == null || $('#area').val() === '\n') {
            alert("Please enter a message to send");
            $('#area').val('');

        }
        else {
           

           // var groupSelected = $("input[name=group]:checked");
            var message = $('#area').val();
           // var uname = setUsername();
            
         
            var room = $('.radio-group').children('.selected').attr('name');
            var data = {
                'selected': room,
                'message': message
            }
            console.log(data);
            jQuery.ajax({
                url: "App/sendMessage",
                type: "POST",
                async: false,
                data: { group: room, messageIn = message },
                cache: false,
                dataType: "JSON",
                success: function (u) {
                    console.log(u);
                    $.connection.myHub.Server.getMessage(u);
                }
            });
         
            $("#CharLeft").css("visibility", "hidden");
            //  console.log("username during connection" + username);
            $('#area').val('');
          
        }
    });

    $.connection.myHub.client.getMessage = function (uid) {
        var dat = {
            'uid' : uid
        }
        $.ajax({
            url: "App/updateDB",
            type: "POST",
            data: dat,
            dataType: 'JSON',
            success: function (groupM) {
                var a = JSON.parse(groupM);
                for (var i = 0; i < a.count; i++) {

                    $("#NewMessage").append(a[i].UserName + "\n" + a[i].message);
                }
            }



        })
    }

    })
    // text counter
    $("#area").on("keydown", function () {

        var length = $("#area").val();
        var lengthNo = length.length;
        var left = 150 - lengthNo;
        console.log(left);
        if (left > 50) {

            $("#CharLeft").css("visibility", "hidden");
            $("#sendText").removeAttr("disabled");
        }
        else if (left > 25 && left <= 50) {

            $("#CharLeft").text(left + " / 150");

            $("#CharLeft").css({ "visibility": "visible", "font-size": "10px", "color": "black" });
            $("#sendText").removeAttr("disabled");
        }
        else if (left > 0 && left <= 25) {

            $("#CharLeft").text(left + " / 150");
            $("#CharLeft").css({ "visibility": "visible", "font-size": "10px", "color": "red" });
            $("#sendText").removeAttr("disabled");
        }
        else {

            $("#CharLeft").text(left + " / 150");
            $("#CharLeft").css({ "visibility": "visible", "font-size": "10px", "color": "red" });
            $("#sendText").attr("disabled", "disabled");
        }

    });

    $("#feedbackMessage").on("keydown", function () {

        var length = $("#feedbackMessage").val();
        var lengthNo = length.length;
        var left = 150 - lengthNo;
        console.log(left);
        if (left > 50) {

            $("#CharRight").css("visibility", "hidden");
            $("#feedbackBtn").removeAttr("disabled");
        }
        else if (left > 25 && left <= 50) {

            $("#CharRight").text(left + " / 150");

            $("#CharRight").css({ "visibility": "visible", "font-size": "10px", "color": "black" });
            $("#feedbackBtn").removeAttr("disabled");
        }
        else if (left > 0 && left <= 25) {

            $("#CharRight").text(left + " / 150");
            $("#CharRight").css({ "visibility": "visible", "font-size": "10px", "color": "red" });
            $("#feedbackBtn").removeAttr("disabled");
        }
        else {

            $("#CharRight").text(left + " / 150");
            $("#CharRight").css({ "visibility": "visible", "font-size": "10px", "color": "red" });
            $("#feedbackBtn").attr("disabled", "disabled");
        }

    });


    function parseJsonDate(jsonDateString) {
        return new Date(parseInt(jsonDateString.replace('/Date(', '')));
    }

    $(".radio-group .radio").click(function () {
        $("#NewMessage").text("");
        $(this).parent().find(".radio").removeClass("selected");
        $(this).addClass("selected");
        var val = $(this).attr('name').toString();
        console.log(val);
        var data = {
            Some: val
        };
        console.log(data);
        jQuery.ajax({
            url: "/App/ViewGroups",
            type: "POST",
            data: {selected : val},
            cache: false,
            success: function (JSmessage) {
                console.log(JSmessage);
                var p = JSON.parse(JSmessage);
                console.log(p);
                //   var a = JSON.parse(p);
                for (var i = (p.length -1); i > 0; i--) {
                        $("#NewMessage").append(p[i].UserName + "<br/>" + p[i].message + "<br/>");
                    }
                    console.log(p);
                    console.log("\n");
            }
            }
            $.post('/Home/ShowGroup', val, function (data) {
                console.log(data);
            });


            //$(this).parent().find('input').val(val);




        })


    });
    

    $.connection.myHub.client.hello = function () {


        $.post("@Url.Action('ReceiveText', 'App')");
        $.ajax({
            type: "POST",
            url: "App/ReceiveText",
            success: function (messageList) {


            }


        });
     
        if (true /*$("input[name='chatRoom']:checked").val().toString() == result.target*/) {
       

            {
                var title = document.title;
                setInterval(timer, 500);
                document.title = title;

                function timer() {
                    document.title = "New Message!";
                }
            } while (document.hasFocus() == false);
            // $(window).focus(function () { })
        }
        //end filtering

    }








    $("#feedbackBtn").on("click", function () {
        console.log("why");
        console.log("button pressed");
        var rating = $("input[name='rating']:checked").val();
        var message = $("#feedbackMessage").val();
        $("#CharRight").css("visibility", "hidden");
        $.connection.myHub.server.feedbackSend(rating, message);

        $("#sendFeed").text("Please hold, your feedback is being submitted");
        console.log("send to server");

    })

    $("#StartChatBtn").on("click", function () {
        // go send select users from friend and group
        // display

        // creates more buttons





    })




    $.connection.myHub.client.sessionUsername = function () { }

    //set slowmode
    function SetSlowMode() {

        $("#sendText").attr("disabled", "disabled");
        $.connection.myHub.server.timer();




    }
    //pinned
    function pinMsg() {



    }

    $.connection.myHub.client.timer = function () {


        $("#sendText").removeAttr("disabled");


    }


    $("#ShowGroups").on("click", function () {


        var u = "2323";
        $.connection.myHub.server.ListGroup(u);





    })
    $.connection.myHub.client.feedbackSend = function (feedMe) {

        var resultOld = JSON.parse(feedMe);
        console.log(resultOld);


        $("input[name='ratin']:checked").val("");
        $("#feedbackMessage").val("");
        $("#sendFeed").text("Your feedback has been submitted. Thank you!");



    }


    $.connection.myHub.client.ListGroup = function (arr) {

        foreach(ob in arr)
        {
            //print each of the obj out in a div and each group is another div     }

        }
    }

        $(".groups").on("click", function () {

            //var nameValue = $('#upper').find('#lower').attr('name');

        })


        $("#startChat").on("click", function (e) {


           
            //e.preventDefault();
            //$.ajax({

            //    url: $(this).attr("href"), // comma here instead of semicolon   
            //    success: function () {

            //    }



            //})

        })
    
})