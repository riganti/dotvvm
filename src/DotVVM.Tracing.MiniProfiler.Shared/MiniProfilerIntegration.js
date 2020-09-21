(function () {
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
    dotvvm.events.spaNavigated.subscribe(miniProfilerUpdate);
    dotvvm.events.staticCommandMethodInvoked.subscribe(miniProfilerUpdate);

    if (!window.performance || !window.performance.timing) return;

    var dotvvmInitialized = false;
    dotvvm.events.init.subscribe(function () {
        try { mPt.end('DotVVM Init'); } catch (e) { console.error(e); }
        dotvvmInitialized = true;
    });

    window.dotvvm.domUtils.onDocumentReady(function () {
        try { mPt.start('DotVVM Init'); } catch (e) { console.error(e); }
    });

    window.document.getElementById('mini-profiler').addEventListener('load', function () {
        window.MiniProfiler.initCondition = function () { return dotvvmInitialized; };
    });
})()