
var uid = "root" //Placeholder - Move to controller
var eventID = 0 //Placeholder - Move to controller
var eventObj //Placeholder - Move to controller
$(document).ready(function () {
	var selectedEvent; //Move to controller

	$("#closePopup").click(function () {
		$('#popup').css('display', 'none');
	});
	$("#closeEditor").click(function () {
		$('#editor').css('display', 'none');
	});
	$("#deleteDis").click(function () {
		$('#calendar').fullCalendar('removeEvents', eventObj.id)
		$('#calendar').fullCalendar('updateEvent', eventObj);
		$('#popup').css('display', 'none');
	});
	$("#applyEdit").click(function () {
		eventObj.title = $('#chName').val();
		eventObj.color = $('#chColor').val();
		eventObj.description = $('#chDesc').val();
		$('#calendar').fullCalendar('updateEvent', eventObj);
		$('#popuptext').text(eventObj.title);
		$("#popuptext").append("<p id=\"id\">" + eventObj.id + "</p>");
		$("#popuptext").append("<p id=\"date\">" + moment(eventObj.start).format('h:mm:ss a') + "</p>");
		$("#popuptext").append("<p id=\"desc\">" + eventObj.description + "</p>");
		$('#editor').css('display', 'none');
	});
	$("#modEvent").click(function () {

		$('#chName').val(eventObj.title);
		$('#chColor').val(eventObj.color);
		$('#chDesc').val(eventObj.description);
		$('#editor').css('display', 'inline');
		$("#editor").css("position", "absolute");
	});
	/* initialize the external events
	-----------------------------------------------------------------*/

	$('#external-events .fc-event').each(function () {

		// store data so the calendar knows to render an event upon drop
		$(this).data('event', {
			title: $.trim($(this).text()), // use the element's text as the event title
			stick: true // maintain when user navigates (see docs on the renderEvent method)
		});

		// make the event draggable using jQuery UI
		$(this).draggable({
			zIndex: 999,
			revert: true,      // will cause the event to go back to its
			revertDuration: 0  //  original position after the drag
		});

	});


	/* initialize the calendar
	-----------------------------------------------------------------*/

	$('#calendar').fullCalendar({
		header: {
			left: 'prev,next today addEventButton',
			center: 'title ',
			right: 'timeline,month,agendaWeek,agendaDay'
		},
		eventLimit: true, // for all non-agenda views
		eventLimitClick: "popover",
		views: {
			agenda: {
				eventLimit: 6// adjust to 6 only for agendaWeek/agendaDay
			}
		},
		schedulerLicenseKey: 'CC-Attribution-NonCommercial-NoDerivatives',
		editable: true,
		navLinks: true,
		selectable: true,
		droppable: true, // this allows things to be dropped onto the calendar
		dragRevertDuration: 0,
		drop: function () {
			// is the "remove after drop" checkbox checked?
			if ($('#drop-remove').is(':checked')) {
				// if so, remove the element from the "Draggable Events" list
				$(this).remove();
			}
		},
		dayClick: function (date, jsEvent, view) {
			var title = prompt('Enter a title');
			if (title !== null) {
				var description = prompt('Enter a description');
				$('#calendar').fullCalendar('renderEvent', {
					title: title,
					start: date,
					id: uid + eventID,
					editable: true,
					description: description,
					allDay: true
				}, true);
			}
			eventID++;
		},
		customButtons: {
			addEventButton: {
				text: 'add event...',
				click: function () {
					$('#createEvent').css('left', event.pageX);      // <<< use pageX and pageY
					$('#createEvent').css('top', event.pageY);
					$('#createEvent').css('display', 'inline');
					$("#createEvent").css("position", "absolute");
				}
			}
		},
		eventClick: function (calEvent, jsEvent, view) {
			eventObj = calEvent;
			//eventObj = $("#calendar").fullCalendar('clientEvents', selectedEvent.id)[0];
			$('#popuptext').text(calEvent.title);
			$("#popuptext").append("<p id=\"id\">" + calEvent.id + "</p>");
			$("#popuptext").append("<p id=\"date\">" + moment(calEvent.start).format('h:mm:ss a') + "</p>");
			$("#popuptext").append("<p id=\"desc\">" + calEvent.description + "</p>");
			//$('#popuptext').append(calEvent.id).attr(id="id");
			console.log(calEvent.id);
			$('#popup').css('left', event.pageX);      // <<< use pageX and pageY
			$('#popup').css('top', event.pageY);
			$('#popup').css('display', 'inline');
			$("#popup").css("position", "absolute");  // <<< also make it absolute!

		},
		eventDragStop: function (event, jsEvent, ui, view) {

			if (isEventOverDiv(jsEvent.clientX, jsEvent.clientY)) {
				$('#calendar').fullCalendar('removeEvents', event._id);
				var el = $("<div class='fc-event'>").appendTo('#external-events-listing').text(event.title);
				el.draggable({
					zIndex: 999,
					revert: true,
					revertDuration: 0
				});
				el.data('event', { title: event.title, id: event.id, stick: true });
			}
		},
		eventRender: function (event, element) {
			console.log("eventrender called!");
			element.qtip({
				content: event.description
			});
		}

	});


	var isEventOverDiv = function (x, y) {

		var external_events = $('#external-events');
		var offset = external_events.offset();
		offset.right = external_events.width() + offset.left;
		offset.bottom = external_events.height() + offset.top;

		// Compare
		if (x >= offset.left
			&& y >= offset.top
			&& x <= offset.right
			&& y <= offset.bottom) { return true; }
		return false;

	};
	loadEvents();


});
