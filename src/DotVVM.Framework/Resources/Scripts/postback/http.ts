import { getVirtualDirectory, getViewModel } from '../dotvvm-base';
import { DotvvmPostbackError } from '../shared-classes';

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
        throw new DotvvmPostbackError({ type: "network" });
    }

    var resultObject;
    try {
        resultObject = await response.json();
    }
    catch (err) {
        throw new DotvvmPostbackError({ type: "invalidJson", responseText: await response.text() });
    }
    
    if (response.status >= 400) {
        throw new DotvvmPostbackError({ type: "serverError", status: response.status, responseObject: resultObject });
    }

    return resultObject;
}

export async function fetchCsrfToken(): Promise<string> {
    let viewModel = getViewModel();
    if (viewModel.$csrfToken == null) {
        try {
            var response = await fetch(getVirtualDirectory() + "/___dotvvm-create-csrf-token___")
        }
        catch (err) {
            console.warn(`CSRF token fetch failed.`);
            throw new DotvvmPostbackError({ type: "network" });
        }

        if (response.status != 200) {
            console.warn(`CSRF token fetch failed. HTTP status: ${response.statusText}`);
            throw new DotvvmPostbackError({ type: "csrfToken" });
        }

        viewModel.$csrfToken = await response.text();
    }
    return viewModel.$csrfToken;
}

export async function retryOnInvalidCsrfToken<TResult>(postbackFunction: () => Promise<TResult>, iteration: number = 0): Promise<TResult>
{
    try {
        var result = await postbackFunction();
        return result;
    }
    catch (err) {
        // if the CSRF token is invalid, retry the postback
        if (err instanceof DotvvmPostbackError) {
            if (err.reason.type === "serverError") {
                if (err.reason.responseObject.action === "invalidCsrfToken") {
                    console.log("Resending postback due to invalid CSRF token.");
                    viewModel.$csrfToken = null;

                    if (iteration < 3) {
                        return await retryOnInvalidCsrfToken(postbackFunction, iteration + 1);
                    }
                }
            }
        }
        throw err;
    }
}

function appendAdditionalHeaders(headers: Headers, additionalHeaders?: { [key: string]: string }) {
    if (additionalHeaders) {
        for (let key of Object.keys(additionalHeaders)) {
            headers.append(key, additionalHeaders[key]);
        }
    }
}
