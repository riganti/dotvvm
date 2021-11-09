﻿(function () {
    function connect() {
        // listen for markup file changes
        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/_dotvvm/hotReloadHub")
            .withAutomaticReconnect()
            .build();
        connection.on("fileChanged", function (paths) {

            // store it in session storage
            if (typeof dotvvm !== "undefined") {
                window.sessionStorage.setItem("dotvvmHotReloadState", JSON.stringify(dotvvm.state));
            }

            // reload
            window.location.reload();
        });
        connection.start()
            .then(function (e) { dotvvm.log.logInfo('DotVVM view hot reload active.', e); })
            .catch(function (e) { dotvvm.log.logWarning('DotVVM view hot reload error!', e); });
    }

    if (typeof dotvvm !== "undefined") {
        dotvvm.events.initCompleted.subscribe(function() {
            // restore state
            var lastState = window.sessionStorage.getItem("dotvvmHotReloadState");
            window.sessionStorage.removeItem("dotvvmHotReloadState");
            if (lastState) {
                dotvvm.patchState(JSON.parse(lastState));
            }

            connect();
        });
    } else {
        // called from error page
        connect();
    }
})();
