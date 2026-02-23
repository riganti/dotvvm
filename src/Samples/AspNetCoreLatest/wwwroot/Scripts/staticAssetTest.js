// Static asset test file
console.log("Static asset test script loaded!");
dotvvm.events.initCompleted.subscribe(function () {
    dotvvm.patchState({ScriptLoaded: true})
});
