import { keys } from "../utils/objects";

export function buildRouteUrl(routePath: string, params: any): string {
    // prepend url with backslash to correctly handle optional parameters at start
    routePath = '/' + routePath;
    const paramPattern =
    /*
         (         ) -- group 1 - prefix
          \/?[^\/{]*  -- match prefix = optional slash plus any string without slashes
                     \{        \} -- in curly braces
                       (      )       -- group 2 - paramName
                        [^\}]+        -- anything except }, more than one character
    */
        /(\/?[^\/{]*)\{([^\}]+)\}/g

    const url = routePath.replace(paramPattern, (s, prefix, paramName) => {
        if (!paramName) {
            return "";
        }
        const x = ko.unwrap(params[paramName])
        return x == null ? "" : prefix + encodeURIComponent(x);
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

    for (const property of keys(query)) {
        if (!property) {
            continue;
        }
        const queryParamValue = ko.unwrap(query[property]);
        if (queryParamValue == null) {
            continue;
        }

        resultSuffix += (resultSuffix.indexOf("?") != -1 ? "&" : "?") + `${encodeURIComponent(property)}=${encodeURIComponent(queryParamValue)}`
    }
    return resultSuffix + hashSuffix;
}
