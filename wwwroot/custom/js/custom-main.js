
/*var jsonAutorization = JSON.parse($("#HashValueAuthorization").val()); */
var userId = ""; 
var KTGeneralBlockUI = function () {

	
}

$(document).ready(function() {+
	// Show Active class when user on current page
	$('.menu-item.menu-accordion').each(function() {
		if ($(this).find('.menu-link').hasClass('active')) {
			$(this).addClass('show');
		}
	});
});

function _blockUI(elems, param) {
	var target = document.querySelector(elems);
	var blockUI = new KTBlockUI(target, {
		overlayClass: "bg-primary bg-opacity-25",
		opacity: 0.1,
		state: 'primary'
	});

	if (param) {
		blockUI.block();
	} else {
		$(elems).removeClass("blockui"); 
	}
}

/*$("#ListNotification").val(); */

function searchNotification(search) {

	var nrp = $("#dskjlaksjuajkh_username").val(); 
	var url = $("#dskjlaksjuajkh_web_api_url").val() + "api/ApiNotification/GetNotificationHistory"; 

	var prams = {
		nrp : nrp, 
		search: $(search).val(), 
	}

	console.log(prams); 

	$("#notificationHistoryLogs").html(''); 
	$.post(url, prams)
		.done(function (response) {
			console.log(response); 
	
	}); 

	//console.log(update); 
}

function notificationEventsGlobal(ths) {

	var machineId = $(ths).data("machine");
	var notifId = $(ths).data("id");
	var nrp = $(ths).data("nrp");
	var url = $(ths).data("url");

	$.post(url, { notif_id: notifId, nrp: nrp, machine_id: machineId }).done(function (response) {
		console.log("response done");
		console.log(response);
		if (response != "") {
			window.open(response, "_self"); 
		}
	}).fail(function (response) {
		console.log("fail!");
		console.log(response);
	});
}


//function setAuthorization() {

//	searchNotification($("#SKhjksajsjSearch")); 
//	console.log("js auth");
//	console.log(jsonAutorization);

//	if (jsonAutorization.AllowCreate == "YES") {
//		$(".btn_create").removeClass("hide");
//	} else {
//		$(".btn_create").addClass("hide");
//	}

//	if (jsonAutorization.AllowDelete == "YES") {
//		$(".btn_delete").removeClass("hide");
//	} else {
//		$(".btn_delete").addClass("hide"); 
//	}

//	if (jsonAutorization.AllowDownload == "YES") {
//		$(".btn_download").removeClass("hide");
//	} else {
//		$(".btn_download").addClass("hide"); 
//	}

//	if (jsonAutorization.AllowUpdate == "YES") {
//		$(".btn_update").removeClass("hide");
//	} else {
//		$(".btn_update").addClass("hide"); 
//	}

//	if (jsonAutorization.AllowView == "YES") {
//		$(".btn_view").removeClass("hide");

//		console.log("allow view"); 
//	} else {
//		$(".btn_view").addClass("hide"); 
//		console.log("hide view"); 
//	}
//}


function onActionForm(form) {
	_blockUI("body", true); 
	$.ajax({
		type: form.attr('method'),
		url: form.attr('action'),
		data: form.serialize()
	}).done(function (data) {
		_blockUI("body", false); 
		if (!data.result.status) {

			Swal.fire({
				title: "Failed",
				text: data.result.message,
				icon: "error",
				buttonsStyling: false,
				customClass: {
					confirmButton: "btn btn-primary"
				}
			}).then(function (confirm) {
				return; 
			});
		} else {
			Swal.fire({
				title: "Successfully",
				text: data.result.message,
				icon: "success",
				buttonsStyling: false,
				customClass: {
					confirmButton: "btn btn-primary"
				}
			}).then(function (confirm) {
				/*toastr.success(data.result.message);*/
				setTimeout(function () {
					window.open(data.result.redirect_url, "_self");
				}, 500);
			});
		}
	}).fail(function (data) {
		toastr.error("Failed to insert data!, Check your connection!");
	});
}
