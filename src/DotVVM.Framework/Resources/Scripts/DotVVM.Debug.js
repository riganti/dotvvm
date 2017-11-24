(function() {
    var div = "<div id='debugWindow'><h1></h1><button type='button' id='closeDebugWindow'>Close</button><iframe></iframe><div id='debugFooter'></div></div>";
    var parser = new DOMParser();
    var debugWindow = parser.parseFromString(div, "text/html").firstElementChild.querySelector("#debugWindow");
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

    var notificationWindow = parser.parseFromString("<div id='debugNotification'></div>", "text/html").firstElementChild.querySelector("#debugnotification");
    // notificationWindow.style.display = "none";
    notificationWindow.style.zIndex = 2147483647;
    notificationWindow.style.position = "fixed";
    notificationWindow.style.top = "0px";
    notificationWindow.style.right = "0px";
    notificationWindow.style.backgroundColor = "darkred";
    notificationWindow.style.color = "white";
    notificationWindow.style.fontSize = "1.0em";
    notificationWindow.style.width = "400px";
    notificationWindow.style.padding =  "20px";
    notificationWindow.style.opacity = 0;
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

    dotvvm.evaluator.tryEval = function(func) {
        try {
            return func();
        } catch (error) {
            console.warn("Error '" + error + "' occurred while evaluating " + func + ".");
            return null;
        }
    }

    dotvvm.events.error.subscribe(function (e) {
        console.error("DotVVM: An " + (e.handled ? "" : "un") + "handled exception returned from the server command.");
        console.log("XmlHttpRequest: ", e.xhr);
        console.log("ViewModel: ", e.viewModel);
        if (e.handled) return;
        debugWindow.querySelector("h1").textContent = "DotVVM Debugger: Error " + (e.xhr.status ? e.xhr.status + ": " + e.xhr.statusText + "" : "XmlHttpRequest failed, maybe internet connection is lost or url is malformed");
        var iframe = debugWindow.querySelector("iframe");
        var iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
        iframeDocument.querySelector('html').innerHTML = e.xhr.responseText;
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
            if (dotvvm.validation.errors().length > 0) {
                displayPostbackAbortedWarning("Postback aborted because validation failed.");
            } else displayPostbackAbortedWarning("Postback interrupted");
        }
        setDebugMapProperty(dotvvm.viewModels[e.viewModelName]);
    });
    dotvvm.events.init.subscribe(function() {
        setDebugMapProperty(dotvvm.viewModels["root"])
    });
})();
