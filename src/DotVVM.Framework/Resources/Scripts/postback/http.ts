import { virtualDirectory, viewModel } from '../dotvvm-root';

export async function getJSON(url: string, spaPlaceHolderUniqueId?: string, additionalHeaders?: { [key: string]: string }): Promise<any> {
    const headers = new Headers();
    headers.append('Accept', 'application/json');
    if (compileConstants.isSpa && spaPlaceHolderUniqueId) {
        headers.append('X-DotVVM-SpaContentPlaceHolder', spaPlaceHolderUniqueId);
    }
    appendAdditionalHeaders(headers, additionalHeaders);
    
    return await fetchJson(url, { headers: headers });
}

export async function postJSON(url: string, postData: any, additionalHeaders?: { [key: string]: string }): Promise<any> {
    const headers = new Headers();
    headers.append('Content-Type', 'application/json');
    headers.append('X-DotVVM-PostBack', 'true');
    appendAdditionalHeaders(headers, additionalHeaders);

    return await fetchJson(url, { body: postData, headers: headers })
}

export async function fetchJson(url: string, init: RequestInit): Promise<any> {
    var response;
    try {
        response = await fetch(url, init);
    }
    catch (err) {
        throw { type: "network" };
    }

    var resultObject;
    try {
        resultObject = await response.json();
    }
    catch (err) {
        throw { type: "json" };
    }
    
    if (response.status >= 400) {
        throw { type: "serverError", status: response.status, resultObject: resultObject };
    }

    return resultObject;
}

export async function fetchCsrfToken(): Promise<string> {
    if (viewModel.$csrfToken == null) {
        try {
            const response = await fetch(virtualDirectory + "/___dotvvm-create-csrf-token___")
            if (response.status != 200)
                throw new Error(`Can't fetch CSRF token: HTTP Status ${response.statusText}`);
            viewModel.$csrfToken = await response.text();
        }
        catch (err) {
            console.warn(`CSRF token fetch failed.`);
            throw { type: 'csrfToken' };
        }
    }
    return viewModel.$csrfToken;
}

function appendAdditionalHeaders(headers: Headers, additionalHeaders?: { [key: string]: string }) {
    if (additionalHeaders) {
        for (let key of Object.keys(additionalHeaders)) {
            headers.append(key, additionalHeaders[key]);
        }
    }
}
