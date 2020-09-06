import * as events from '../events';
import * as magicNavigator from '../utils/magic-navigator'
import { handleSpaNavigationCore } from "../spa/spa";
import { disablePostbacks } from './gate';

export function performRedirect(url: string, replace: boolean, allowSpa: boolean): void {
    disablePostbacks();

    if (replace) {
        location.replace(url);
    } else if (compileConstants.isSpa && allowSpa) {
        handleSpaNavigationCore(url)
    } else {
        magicNavigator.navigate(url);
    }
}

export function handleRedirect(options: PostbackOptions, resultObject: any, response: Response, replace: boolean = false): void {
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

    performRedirect(url, replace, resultObject.allowSpa);
}
