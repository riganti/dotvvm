import { events } from "../DotVVM.Events";
import * as magicNavigator from '../utils/magic-navigator'
import { handleSpaNavigationCore } from "../spa/spa";

export function performRedirect(url: string, replace: boolean, allowSpa: boolean): Promise<DotvvmNavigationEventArgs> | undefined {
    if (replace) {
        location.replace(url);
    }

    else if (compileConstants.isSpa && allowSpa) {
        return handleSpaNavigationCore(url)
    }
    else {
        magicNavigator.navigate(url);
    }
}

export function handleRedirect(resultObject: any, replace: boolean = false): Promise<DotvvmNavigationEventArgs> | undefined {
    if (resultObject.replace != null) replace = resultObject.replace;
    const url = resultObject.url;

    // trigger redirect event
    const redirectArgs : DotvvmRedirectEventArgs = {
        viewModel: dotvvm.viewModels["root"],
        viewModelName: "root",
        url,
        replace,
    }
    events.redirect.trigger(redirectArgs);

    return performRedirect(url, replace, resultObject.allowSpa);
}
