// script which makes error page a bit better, but it also works without JS

fillCookieTable()

document.getElementById("save-and-share-button").addEventListener("click", saveAndShare)

function fillCookieTable() {
	// fill in cookie values, since we cannot place them from the server for security reasons
	var cookies = {}
	document.cookie.split(';').forEach(function(c) {
		let split = c.split('=', 2);
		cookies[split[0].trim()] = split[1];
	})
	var table = document.querySelector('.cookie-table');
	var rows = table.tBodies[0].rows;
	for (var i = 0; i < rows.length; i++) {
		var cookieName = rows[i].cells[0].textContent.trim();
		if (cookieName in cookies) {
			rows[i].cells[1].textContent = cookies[cookieName]
		} else {
			rows[i].cells[1].classList.add('hint-text')
		}
	}
}

function saveAndShare() {
	// saves the page as HTML
	var pageAsHtml = document.documentElement.outerHTML;
    var blob = new Blob([pageAsHtml], {type:'text/html'});
    var downloadLink = document.createElement("a");
	var exceptionName = document.querySelector(".exceptionType")
	if (exceptionName) {
		exceptionName = exceptionName.textContent.replace(/^.*\./, "")
		downloadLink.download = "dotvvm-error-" + exceptionName + ".html";
	} else {
		downloadLink.download = "dotvvm-error.html";
	}
    downloadLink.href = window.URL.createObjectURL(blob);
    downloadLink.click();
}
