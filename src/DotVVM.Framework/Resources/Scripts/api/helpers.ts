export function basicAuthenticatedFetch(input: RequestInfo, init: RequestInit): Promise<any> {
    function requestAuth() {
        const a = prompt("You credentials for " + ((<any> input)["url"] || input)) || "";
        sessionStorage.setItem("dotvvm-api-fetch-basic", a);
        return a;
    }
    const auth = sessionStorage.getItem("dotvvm-api-fetch-basic");
    if (init == null) {
        init = {};
    }
    if (auth != null) {
        if (init.headers == null) {
            init.headers = {};
        }
        if (!(<any> init.headers)['Authorization']) {
            (<any> init.headers)["Authorization"] = 'Basic ' + btoa(auth);
        }
    }
    if (!init.cache) {
        init.cache = "no-cache";
    }
    return window.fetch(input, init).then(response => {
        if (response.status === 401 && auth == null) {
            if (sessionStorage.getItem("dotvvm-api-fetch-basic") == null) {
                requestAuth();
            }
            return basicAuthenticatedFetch(input, init);
        } else {
            return response;
        }
    });
}
