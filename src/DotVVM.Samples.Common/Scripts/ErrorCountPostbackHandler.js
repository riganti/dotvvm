dotvvm.events.init.subscribe(function () {
    dotvvm.postbackHandlers["errorCount"] = function (options) {
        let element = document.getElementById(options.resultId);

        return {
            execute: function (callback) {
                return new Promise(function (resolve, reject) {
                    callback().then(resolve, reject);
                    element.innerText = dotvvm.validation.errors().length.toString();
                });
            }
        };
    };
});
