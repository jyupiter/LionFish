$(function () {
	$.connection.hub.url = "https://localhost:44393/signalr";
	$.connection.hub.start()
		.done(function () { })
		.fail(function () { })

	$("#feedbackBtn").on("click", function () {

		var rate = $("input[name='rating']:checked").val();
		var feed = $("#feedbackMessage").val();
		console.log("before");

		$.connection.myHub.server.feedbackSend(rate, feed);



		$("#sendFeed").val("Your feedback is being submitted. Please hold.");




	});





	$.connection.myHub.client.feedbackSend = function () {

		console.log("after");

		$("#feedbackMessage").val("");
		$("#sendFeed").text("Thank you! Your feedback has been submitted for review. Thank you for using LionFish web services.");

	}


});