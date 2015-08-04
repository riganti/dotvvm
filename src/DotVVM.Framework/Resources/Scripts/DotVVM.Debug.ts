var debugWindow = $(document.body)
    .append("<div id='debugWindow'><h1></h1><button type='button' id='closeDebugWindow'>Close</button><iframe /><div id='debugFooter'></div></div>")
    .find("#debugWindow");
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

debugWindow.find("#closeDebugWindow")
    .click(() => debugWindow.css({ display: "none" }))
    .css({
    position: "absolute",
    top: 0,
    right: 0
});
debugWindow.find("#debugFooter")
    .css({ flex: "0 1 auto" });
debugWindow.find("h1")
    .css({ flex: "0 1 auto" });
debugWindow.find("iframe").css({
    flex: "1 1 auto",
    width: "100%"
});

dotvvm.tryEval = function (func: () => any) {
    try
    {
        return func()
    }
    catch (error) {
        console.log(`error '${ error }' occured in ${ func }`);
        return null;
    }
}


dotvvm.events.error.subscribe(e => {
    if (e.handled) return;
    console.log("DotVVM: An unhandled exception returned from the server command.");
    console.log("XmlHttpRequest: ", e.xhr);
    console.log("ViewModel: ", e.viewModel);
    debugWindow.find("h1").text("DotVVM Debugger: Error " + (e.xhr.status ? e.xhr.status + ": " + e.xhr.statusText + "" : "(unknown)"));
    debugWindow.find("iframe").contents().find('html').html(e.xhr.responseText);
    debugWindow.css({ display: "flex" });
    e.handled = true;
});

function setDebugMapProperty(obj) {
    Object.defineProperty(obj, "$debugMap", {
        enumerable: false,
        configurable: true,
        get: () => dotvvm.serialization.serialize(obj)
    });
}

dotvvm.events.afterPostback.subscribe(e => setDebugMapProperty(dotvvm.viewModels[e.viewModelName]));
dotvvm.events.init.subscribe(e => setDebugMapProperty(dotvvm.viewModels["root"]));
