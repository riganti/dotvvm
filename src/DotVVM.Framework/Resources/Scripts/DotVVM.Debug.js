(function() {
    var div = "<div id='debugWindow'><h1></h1><button type='button' id='closeDebugWindow'>Close</button><iframe></iframe><div id='debugFooter'></div></div>";
    var parser = new DOMParser();
    var debugWindow = parser.parseFromString(div, "text/html").querySelector("#debugWindow");
    debugWindow.style.display = 'none';
    debugWindow.style.flexFlow = "column";
    debugWindow.style.zIndex = 2147483647;
    debugWindow.style.position = "fixed";
    debugWindow.style.width = "100%";
    debugWindow.style.height = "100vh";
    debugWindow.style.backgroundColor = "white";
    debugWindow.style.top = 0;
    debugWindow.style.left = 0;
    document.body.appendChild(debugWindow);

    var notificationWindow = parser.parseFromString("<div id='debugNotification'></div>", "text/html").querySelector("#debugNotification");
    notificationWindow.style.display = "none";
    notificationWindow.style.zIndex = 2147483647;
    notificationWindow.style.position = "fixed";
    notificationWindow.style.top = "0px";
    notificationWindow.style.right = "0px";
    notificationWindow.style.backgroundColor = "darkred";
    notificationWindow.style.color = "white";
    notificationWindow.style.fontSize = "1.0em";
    notificationWindow.style.width = "400px";
    notificationWindow.style.padding = "20px";
    document.body.appendChild(notificationWindow);

    notificationWindow.addEventListener("click", function() {
        setTimeout(function () {
                notificationWindow.style.display = "none";
        }, 200);
    });

    var closeDebugWindow = debugWindow.querySelector("#closeDebugWindow");
    closeDebugWindow.addEventListener("click", function() {
        debugWindow.style.display = "none";
    });

    closeDebugWindow.style.position = "absolute";
    closeDebugWindow.style.top = 0;
    closeDebugWindow.style.right = 0;

    var debugFooter = debugWindow.querySelector("#debugFooter");
    debugFooter.style.flex = "0 1 auto";

    var h1 = debugWindow.querySelector("h1");
    h1.style.flex = "0 1 auto";

    var iframe = debugWindow.querySelector("iframe");

    iframe.style.flex = "1 100 auto";
    iframe.style.width = "100%";

    dotvvm.events.error.subscribe(function (e) {
        console.error("DotVVM: An " + (e.handled ? "" : "un") + "handled exception returned from the server command.");
        console.log("Response: ", e.response);
        console.log("ViewModel: ", e.viewModel);
        if (e.handled) return;
        debugWindow.querySelector("h1").textContent = "DotVVM Debugger: Error " +
           (e.response && e.response.status ? e.response.status + ": " + e.response.statusText + "" :
            e.responseObject ? "DotVVM error response" :
            "HTTP request failed, maybe internet connection is lost or url is malformed");
        var iframe = debugWindow.querySelector("iframe");
        var iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
        if (e.responseObject) {
            iframeDocument.querySelector('body').innerHTML = "<code><pre></pre></code>";
            iframeDocument.querySelector('pre').innerText = JSON.stringify(e.responseObject, null, "   ");
        } else if (e.response && e.response.bodyUsed) {
            iframeDocument.querySelector('html').innerText = "Server returned something, but the resource body was already used by another handler. You can use your browser's devtools to inspect the request content.";
        } else if (e.response) {
            iframeDocument.querySelector('html').innerHTML = "";
            e.response.text().then(function (text) {
                iframeDocument.querySelector('html').innerHTML = text;
            });
        } else {
            iframeDocument.querySelector('html').innerHTML = "";
        }
        // debugWindow.height = window.innerHeight;
        debugWindow.style.display = "flex";
        e.handled = true;
    });

    function setDebugMapProperty(obj) {
        Object.defineProperty(obj, "$debugMap", {
            enumerable: false,
            configurable: true,
            get: function() {
                return dotvvm.serialization.serialize(obj)
            }
        });
    }

    function displayPostbackAbortedWarning(message) {
        notificationWindow.style.display = "block";
        notificationWindow.style.opacity = 0;
        setTimeout(function () {
            notificationWindow.textContent = message;
            notificationWindow.style.transition = "opacity 0.5s"
            notificationWindow.style.opacity = 1;

            setTimeout(function() {
                notificationWindow.style.transition = "opacity 1s"
                notificationWindow.style.opacity = 0;
                setTimeout(function () {
                    notificationWindow.style.display = "none";
                }, 1000);
            }, 7000)
        }, 0)
    }

    dotvvm.events.afterPostback.subscribe(function (e) {
        if (e.wasInterrupted) {
            if (dotvvm.validation.errors.length > 0) {
                displayPostbackAbortedWarning("Postback aborted because validation failed.");
            } else displayPostbackAbortedWarning("Postback interrupted");
        }
        setDebugMapProperty(dotvvm.viewModels.root);
    });
    dotvvm.events.init.subscribe(function() {
        setDebugMapProperty(dotvvm.viewModels.root)
    });

    for (var event in dotvvm.events) {
        if ("subscribe" in dotvvm.events[event]) {
            (function (event) {
                dotvvm.events[event].subscribe(function (e) {
                    console.log("Event " + event, e);
                });
            })(event);
        }
    }
})();
