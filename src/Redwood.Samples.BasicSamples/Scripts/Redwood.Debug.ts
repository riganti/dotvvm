var debugWindow = $(document).append("<div id='debugWindow'><h1></h1><iframe id='debugIframe' /></div><button type='button' id='closeDebugWindow'>Close</button>");
debugWindow.css({
    display: "none",
    zLevel: 10000001,
    position: "fixed",
    width: "100%",
    height: "100%",
});
debugWindow.find("#closeDebugWindow").click(() => debugWindow.css({ display: "none" }));

redwood.events.error.subscribe(e => {
    console.log("error has occured");
    console.log("xhr: ");
    console.log(e.xhr);
    console.log("viewModel: ");
    console.log(e.viewModel);
    debugWindow.find("h1").text("Error " + (e.xhr.status ? e.xhr.status + ": " + e.xhr.statusText + "" : ""));
    debugWindow.find("iframe").contents().find('html').html(e.xhr.responseText);
    debugWindow.css({ display: "block" });
    return true;
});