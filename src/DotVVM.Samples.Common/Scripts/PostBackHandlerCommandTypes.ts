declare var dotvvm: any;

dotvvm.events.init.subscribe(() => {
    dotvvm.postBackHandlers["PostBackHandlerCommandTypes"] = handlerOptions => ({
        execute(callback, postbackOptions) {
            return new Promise(async (resolve, reject) => {
                let element = postbackOptions.sender;
                
                try {
                    element.classList.add("pending");

                    let postbackCommit = await callback();
                    resolve(postbackCommit);
                    
                    element.classList.remove("pending");
                    element.classList.add("success");
                } catch (e) {
                    element.classList.remove("pending");
                    element.classList.add("error");
                    
                    reject(e);
                }
            });
        }
    });
});
