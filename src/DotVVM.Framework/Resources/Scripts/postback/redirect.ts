import * as events from '../events';
import * as magicNavigator from '../utils/magic-navigator'
import { handleSpaNavigationCore } from "../spa/spa";
import { disablePostbacks } from './gate';

export function performRedirect(url: string, replace: boolean, allowSpa: boolean): Promise<DotvvmNavigationEventArgs> | undefined {
    disablePostbacks();
    
    if (replace) {
        location.replace(url);
    } else if (compileConstants.isSpa && allowSpa) {
        return handleSpaNavigationCore(url)
    } else {
        magicNavigator.navigate(url);
    }
}

export function handleRedirect(resultObject: any, replace: boolean = false): Promise<DotvvmNavigationEventArgs> | undefined {
    if (resultObject.replace != null) {
        replace = resultObject.replace || replace;
    }
    const url = resultObject.url;

    // trigger redirect event
    const redirectArgs: DotvvmRedirectEventArgs = {
        url,
        replace,
    }
    events.redirect.trigger(redirectArgs);

    return performRedirect(url, replace, resultObject.allowSpa);
}
