dotvvm.events.init.subscribe(function () {
    dotvvm.postbackHandlers["stopwatch"] = function (options) {
        let element = document.getElementById(options.resultId);

        return {
            execute: function (callback) {
                return new Promise(function (resolve, reject) {
                    let startTime = new Date();
                    callback().then(resolve, reject);
                    let endTime = new Date();
                    let length = endTime - startTime;
                    element.innerText = length.toString();
                });
            }
        };
    };
});
