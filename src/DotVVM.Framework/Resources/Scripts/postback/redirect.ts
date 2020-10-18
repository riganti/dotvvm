import * as events from '../events';
import * as magicNavigator from '../utils/magic-navigator'
import { handleSpaNavigationCore } from "../spa/spa";
import { disablePostbacks } from './gate';

export function performRedirect(url: string, replace: boolean, allowSpa: boolean): Promise<any> {
    disablePostbacks();

    if (replace) {
        location.replace(url);
        return Promise.resolve();
    } else if (compileConstants.isSpa && allowSpa) {
        return handleSpaNavigationCore(url);
    } else {
        magicNavigator.navigate(url);
        return Promise.resolve();
    }
}

export async function handleRedirect(options: PostbackOptions, resultObject: any, response: Response, replace: boolean = false): Promise<DotvvmRedirectEventArgs> {
    if (resultObject.replace != null) {
        replace = resultObject.replace || replace;
    }
    const url = resultObject.url;

    // trigger redirect event
    const redirectArgs: DotvvmRedirectEventArgs = {
        ...options,
        url,
        replace,
        serverResponseObject: resultObject,
        response: response
    }
    events.redirect.trigger(redirectArgs);

    await performRedirect(url, replace, resultObject.allowSpa);

    return redirectArgs;
}
