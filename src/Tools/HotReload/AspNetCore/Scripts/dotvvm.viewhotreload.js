dotvvm.events.initCompleted.subscribe(function () {

    // restore state
    var lastState = window.sessionStorage.getItem("dotvvmViewHotReloadState");
    window.sessionStorage.removeItem("dotvvmViewHotReloadState");
    if (lastState) {
        dotvvm.patchState(JSON.parse(lastState));
    }

    // listen for markup file changes
    var connection = new signalR.HubConnectionBuilder().
        withUrl("/_diagnostics/dotvvmViewHotReloadHub").
        withAutomaticReconnect().
        build();
    connection.on("fileChanged", function (paths) {

        // store it in session storage
        window.sessionStorage.setItem("dotvvmViewHotReloadState", JSON.stringify(dotvvm.state));

        // reload
        window.location.reload();
    });
    connection.start().
        then(function (e) { dotvvm.log.logInfo('DotVVM view hot reload active.', e); }).
        catch(function (e) { dotvvm.log.logWarning('DotVVM view hot reload error!', e); });

});