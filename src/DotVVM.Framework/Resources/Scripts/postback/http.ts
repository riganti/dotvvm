import { getVirtualDirectory, getViewModel } from '../dotvvm-base';
import { DotvvmPostbackError } from '../shared-classes';

export async function getJSON<T>(url: string, spaPlaceHolderUniqueId?: string, additionalHeaders?: { [key: string]: string }): Promise<T> {
    const headers = new Headers();
    headers.append('Accept', 'application/json');
    if (compileConstants.isSpa && spaPlaceHolderUniqueId) {
        headers.append('X-DotVVM-SpaContentPlaceHolder', spaPlaceHolderUniqueId);
    }
    appendAdditionalHeaders(headers, additionalHeaders);

    return await fetchJson<T>(url, { headers: headers });
}

export async function postJSON<T>(url: string, postData: any, additionalHeaders?: { [key: string]: string }): Promise<T> {
    const headers = new Headers();
    headers.append('Content-Type', 'application/json');
    headers.append('X-DotVVM-PostBack', 'true');
    appendAdditionalHeaders(headers, additionalHeaders);

    return await fetchJson<T>(url, { body: postData, headers: headers, method: "POST" });
}

export async function fetchJson<T>(url: string, init: RequestInit): Promise<T> {
    let response: Response;
    try {
        response = await fetch(url, init);
    }
    catch (err) {
        throw new DotvvmPostbackError({ type: "network", err });
    }

    const errorResponse = response.status >= 400;
    const isJson = response.headers.get("content-type") && response.headers.get('content-type').includes('application/json');

    if (errorResponse || !isJson) {
        response.statusText
        throw new DotvvmPostbackError({ type: "serverError", status: response.status, responseObject: (isJson ? await response.json() : null), response });
    }

    return response.json();
}

export async function fetchCsrfToken(): Promise<string> {
    const viewModel = getViewModel();
    if (viewModel.$csrfToken == null) {
        let response;
        try {
            response = await fetch(getVirtualDirectory() + "/___dotvvm-create-csrf-token___")
        }
        catch (err) {
            console.warn(`CSRF token fetch failed.`);
            throw new DotvvmPostbackError({ type: "network", err });
        }

        if (response.status != 200) {
            console.warn(`CSRF token fetch failed. HTTP status: ${response.statusText}`);
            throw new DotvvmPostbackError({ type: "csrfToken" });
        }

        viewModel.$csrfToken = await response.text();
    }
    return ko.unwrap(viewModel.$csrfToken);
}

export async function retryOnInvalidCsrfToken<TResult>(postbackFunction: () => Promise<TResult>, iteration: number = 0): Promise<TResult> {
    try {
        const result = await postbackFunction();
        return result;
    }
    catch (err) {
        // if the CSRF token is invalid, retry the postback
        if (err instanceof DotvvmPostbackError) {
            if (err.reason.type === "serverError") {
                if (err.reason.responseObject?.action === "invalidCsrfToken") {
                    console.log("Resending postback due to invalid CSRF token.");
                    getViewModel().$csrfToken = undefined;

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
        for (const key of Object.keys(additionalHeaders)) {
            headers.append(key, additionalHeaders[key]);
        }
    }
}
