
export function buildRouteUrl(routePath: string, params: any): string {
    // prepend url with backslash to correctly handle optional parameters at start
    routePath = '/' + routePath;

    const url = routePath.replace(/(\/[^\/]*?)\{([^\}]+?)\??(:(.+?))?\}/g, (s, prefix, paramName, _, type) => {
        if (!paramName) {
            return "";
        }
        const x = ko.unwrap(params[paramName.toLowerCase()])
        return x == null ? "" : prefix + x;
    });

    if (url.indexOf('/') === 0) {
        return url.substring(1);
    }
    return url;
}

export function buildUrlSuffix(urlSuffix: string, query: any): string {
    const hashIndex = urlSuffix.indexOf("#");
    let resultSuffix = hashIndex != -1 ? urlSuffix.substring(0, hashIndex) : urlSuffix;
    const hashSuffix = hashIndex != -1 ? urlSuffix.substring(hashIndex) : "";

    for (const property of Object.keys(query)) {
        if (!property) {
            continue;
        }
        const queryParamValue = ko.unwrap(query[property]);
        if (queryParamValue == null) {
            continue;
        }

        resultSuffix += (resultSuffix.indexOf("?") != -1 ? "&" : "?") + `${property}=${queryParamValue}`
    }
    return resultSuffix + hashSuffix;
}
