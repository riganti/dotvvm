import { getVirtualDirectory } from '../dotvvm-base';

export function removeVirtualDirectoryFromUrl(url: string): string {
    const virtualDirectory = "/" + getVirtualDirectory();
    if (url.indexOf(virtualDirectory) == 0) {
        return addLeadingSlash(url.substring(virtualDirectory.length));
    } else {
        return url;
    }
}

export function addVirtualDirectoryToUrl(appRelativeUrl: string): string {
    return addLeadingSlash(concatUrl(getVirtualDirectory(), addLeadingSlash(appRelativeUrl)));
}

export function addLeadingSlash(url: string): string {
    if (url.length > 0 && url.substring(0, 1) != "/") {
        return "/" + url;
    }
    return url;
}

export function concatUrl(url1: string, url2: string): string {
    if (url1.length > 0 && url1.substring(url1.length - 1) == "/") {
        url1 = url1.substring(0, url1.length - 1);
    }
    return url1 + addLeadingSlash(url2);
}
