(function () {
    console.log("Initializing the MiniProfiler control");
    var miniProfilerUpdate = function (arg) {
        if (arg.response && arg.response.headers) {
            var jsonIds = arg.response.headers.get('X-MiniProfiler-Ids');
            if (jsonIds) {
                var ids = JSON.parse(jsonIds);
                MiniProfiler.fetchResults(ids);
            }
        }
    };
    dotvvm.events.afterPostback.subscribe(miniProfilerUpdate);
    dotvvm.events.staticCommandMethodInvoked.subscribe(miniProfilerUpdate);

    if (dotvvm.events.spaNavigated) {
        dotvvm.events.spaNavigated.subscribe(miniProfilerUpdate);
    }
    if (!window.performance || !window.performance.timing)
        return;

    var dotvvmInitialized = false;
    dotvvm.events.init.subscribe(_ => {
        try {
            mPt.start('DotVVM Init');
        } catch (e) {
            console.error(e);
        }
    });
    dotvvm.events.initCompleted.subscribe(_ => {
        dotvvmInitialized = true;
        try {
            mPt.end('DotVVM Init');
        } catch (e) {
            console.error(e);
        }
    });

    window.document.getElementById('mini-profiler').addEventListener('load', function () {
        window.MiniProfiler.initCondition = function () { return dotvvmInitialized; };
    });
})()
