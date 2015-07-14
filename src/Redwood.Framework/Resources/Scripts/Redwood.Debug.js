var debugWindow = $(document.body).append("<div id='debugWindow'><h1></h1><button type='button' id='closeDebugWindow'>Close</button><iframe /><div id='debugFooter'></div></div>").find("#debugWindow");
debugWindow.css({
    display: "none",
    flexFlow: "column",
    zLevel: 2147483647,
    position: "fixed",
    width: "100%",
    height: "100%",
    backgroundColor: "white",
    top: 0
});
debugWindow.find("#closeDebugWindow").click(function () { return debugWindow.css({ display: "none" }); }).css({
    position: "absolute",
    top: 0,
    right: 0
});
debugWindow.find("#debugFooter").css({ flex: "0 1 auto" });
debugWindow.find("h1").css({ flex: "0 1 auto" });
debugWindow.find("iframe").css({
    flex: "1 1 auto",
    width: "100%"
});
redwood.events.error.subscribe(function (e) {
    if (e.handled)
        return;
    console.log("Redwood: An unhandled exception returned from the server command.");
    console.log("XmlHttpRequest: ", e.xhr);
    console.log("ViewModel: ", e.viewModel);
    debugWindow.find("h1").text("Redwood Debugger: Error " + (e.xhr.status ? e.xhr.status + ": " + e.xhr.statusText + "" : "(unknown)"));
    debugWindow.find("iframe").contents().find('html').html(e.xhr.responseText);
    debugWindow.css({ display: "flex" });
    e.handled = true;
});
function setDebugMapProperty(obj) {
    Object.defineProperty(obj, "$debugMap", {
        enumerable: false,
        configurable: true,
        get: function () { return ko.mapper.toJS(obj); }
    });
}
redwood.events.afterPostback.subscribe(function (e) { return setDebugMapProperty(redwood.viewModels[e.viewModelName]); });
redwood.events.init.subscribe(function (e) { return setDebugMapProperty(redwood.viewModels["root"]); });
//# sourceMappingURL=Redwood.Debug.js.map