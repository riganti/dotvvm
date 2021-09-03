(function () {
    console.log("Initializing miniprofiler control.");
    var miniProfilerUpdate = function (arg) {
        if (arg.xhr && arg.xhr.getResponseHeader) {
            var jsonIds = arg.xhr.getResponseHeader('X-MiniProfiler-Ids');
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
    if (!window.performance || !window.performance.timing) return;

    var dotvvmInitialized = false;
    
    var initDotvvm = dotvvm.init;
    var initWrapper = function () {
        try { mPt.start('DotVVM Init'); } catch (e) { console.error(e); }
        dotvvm.init = initDotvvm;
        var init = initDotvvm.apply(this, arguments);
        try { mPt.end('DotVVM Init'); } catch (e) { console.error(e); }
        dotvvmInitialized = true;
        return init;
    }
    dotvvm.init = initWrapper;

    window.document.getElementById('mini-profiler').addEventListener('load', function () {
        window.MiniProfiler.initCondition = function () { return dotvvmInitialized; };
    });
})()