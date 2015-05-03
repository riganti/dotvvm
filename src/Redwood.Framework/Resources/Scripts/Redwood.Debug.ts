var debugWindow = $(document.body)
    .append("<div id='debugWindow'><h1></h1><iframe /><div id='debugFooter'><button type='button' id='closeDebugWindow'>Close</button></div></div>")
    .find("#debugWindow");
debugWindow.css({
    display: "none",
    flexFlow: "column",
    zLevel: 10000001,
    position: "fixed",
    width: "100%",
    height: "100%",
    backgroundColor: "white",
    top: 0
});

debugWindow.find("#closeDebugWindow")
    .click(() => debugWindow.css({ display: "none" }));
debugWindow.find("#debugFooter")
    .css({ flex: "0 1 auto" });
debugWindow.find("h1")
    .css({ flex: "0 1 auto" });
debugWindow.find("iframe").css({
    flex: "1 1 auto",
    width: "100%"
});


redwood.events.error.subscribe(e => {
    if (e.handled) return;
    console.log("Redwood: An unhandled exception returned from the server command.");
    console.log("XmlHttpRequest: ", e.xhr);
    console.log("ViewModel: ", e.viewModel);
    debugWindow.find("h1").text("Redwood Debugger: Error " + (e.xhr.status ? e.xhr.status + ": " + e.xhr.statusText + "" : "(unknown)"));
    debugWindow.find("iframe").contents().find('html').html(e.xhr.responseText);
    debugWindow.css({ display: "flex" });
    e.handled = true;
});

redwood.events.afterPostback.subscribe(e => {
    Object.defineProperty(redwood.viewModels[e.viewModelName], "$debugMap", {
        enumerable: false,
        configurable: true,
        get: () => ko.mapper.toJS(redwood.viewModels[e.viewModelName])
    });
});
