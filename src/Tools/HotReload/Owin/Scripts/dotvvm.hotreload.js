(function() {
    function connect() {
        // listen for markup file changes
        var hub = $.connection.dotvvmHotReloadHub;
        hub.client.fileChanged = function(virtualPaths) {

            // store it in session storage
            if (typeof dotvvm !== "undefined") {
                window.sessionStorage.setItem("dotvvmHotReloadState", JSON.stringify(dotvvm.state));
            }

            // reload
            window.location.reload();
        };
        $.connection.hub.start()
            .done(function() { console.log('DotVVM view hot reload active.'); })
            .fail(function(e) { console.warn('DotVVM view hot reload error!', e); });
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
