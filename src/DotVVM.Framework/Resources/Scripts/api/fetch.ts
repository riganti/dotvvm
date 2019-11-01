import api from './api'; 

export function basicAuth(input: RequestInfo, init: RequestInit) : Promise<any> {
    function requestAuth() {
        var a = prompt("You credentials for " + (input["url"] || input)) || "";
        sessionStorage.setItem("dotvvm-api-fetch-basic", a);
        return a;
    }
    var auth = sessionStorage.getItem("dotvvm-api-fetch-basic");
    if (init == null) init = {};
    if (auth != null)
    {
        if (init.headers == null) init.headers = {};
        if (init.headers['Authorization'] == null) init.headers["Authorization"] = 'Basic ' + btoa(auth);
    }
    if (!init.cache) init.cache = "no-cache";
    return window.fetch(input, init).then(response => {
        if (response.status === 401 && auth == null) {
            if (sessionStorage.getItem("dotvvm-api-fetch-basic") == null) requestAuth();
            return basicAuth(input, init);
        } else {
            return response;
        }
    });
}

api.fetch = export;
