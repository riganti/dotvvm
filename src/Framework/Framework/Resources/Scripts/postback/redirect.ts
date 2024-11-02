import * as events from '../events';
import * as magicNavigator from '../utils/magic-navigator'
import { handleSpaNavigationCore } from "../spa/spa";
import { delay } from '../utils/promise';


export function performRedirect(url: string, replace: boolean, allowSpa: boolean): Promise<any> {
    if (replace) {
        location.replace(url);
    } else if (compileConstants.isSpa && allowSpa) {
        return handleSpaNavigationCore(url);
    } else {
        magicNavigator.navigate(url);
    }

    // When performing redirect, we pretend that the request takes additional X second to avoid
    // double submit with Postback.Concurrency=Deny or Queue.
    // We do not want to block the page forever, as the redirect might just return a file (or HTTP 204/205),
    // and the page will continue to live.
    return delay(5_000);
}

export async function handleRedirect(options: PostbackOptions, resultObject: any, response: Response, replace: boolean = false): Promise<DotvvmRedirectEventArgs> {
    replace = Boolean(resultObject.replace) || replace;
    const url = resultObject.url;

    // trigger redirect event
    const redirectArgs: DotvvmRedirectEventArgs = {
        ...options,
        url,
        replace,
        serverResponseObject: resultObject,
        response: response,
    }
    events.redirect.trigger(redirectArgs);

    const downloadFileName = resultObject.download
    if (downloadFileName != null) {
        magicNavigator.navigate(url, downloadFileName)
    } else {
        await performRedirect(url, replace, resultObject.allowSpa);
    }

    return redirectArgs;
}
