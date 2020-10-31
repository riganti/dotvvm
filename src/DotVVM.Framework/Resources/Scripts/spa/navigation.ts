import * as postback from '../postback/postback';
import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import { loadResourceList } from '../postback/resourceLoader';
import * as updater from '../postback/updater';
import * as events from './events';
import { getSpaPlaceHoldersUniqueId, isSpaReady } from './spa';
import { handleRedirect } from '../postback/redirect';
import * as gate from '../postback/gate';
import { DotvvmPostbackError } from '../shared-classes';

let lastStartedNavigation = -1

export async function navigateCore(url: string, options: PostbackOptions, handlePageNavigating: (url: string) => void): Promise<DotvvmNavigationEventArgs> {
    
    let response: http.WrappedResponse<any> | undefined;

    try {
        // trigger spaNavigating event
        const spaNavigatingArgs: DotvvmSpaNavigatingEventArgs = {
            ...options,
            url,
            cancel: false
        };
        events.spaNavigating.trigger(spaNavigatingArgs);
        if (spaNavigatingArgs.cancel) {
            throw new DotvvmPostbackError({ type: "event" });
        }

        lastStartedNavigation = options.postbackId
        gate.disablePostbacks()

        // compose URLs
        // TODO: get rid of ___dotvvm-spa___
        const spaFullUrl = uri.addVirtualDirectoryToUrl("/___dotvvm-spa___" + uri.addLeadingSlash(url));
        const displayUrl = uri.addVirtualDirectoryToUrl(url);

        // send the request
        response = await http.getJSON<any>(spaFullUrl, getSpaPlaceHoldersUniqueId());

        // if another postback has already been passed, don't do anything
        if (options.postbackId < lastStartedNavigation) {
            return <DotvvmNavigationEventArgs> { }; // TODO: what here https://github.com/riganti/dotvvm/pull/787/files#diff-edefee5e25549b2a6ed0136e520e009fR852
        }

        // use custom browser navigation function
        if (handlePageNavigating) {
            handlePageNavigating(displayUrl);
        }

        await loadResourceList(response.result.resources);

        if (response.result.action === "successfulCommand") {
            updater.updateViewModelAndControls(response.result);
            isSpaReady(true);
        } else if (response.result.action === "redirect") {
            await handleRedirect(options, response.result, response.response!);
            return { ...options, url };
        }

        // trigger spaNavigated event
        const spaNavigatedArgs: DotvvmSpaNavigatedEventArgs = {
            ...options,
            url,
            viewModel: getViewModel(),
            serverResponseObject: response.result,
            response: response.response
        };
        events.spaNavigated.trigger(spaNavigatedArgs);

        return spaNavigatedArgs;

    } catch (err) {
        // trigger spaNavigationFailed event
        let spaNavigationFailedArgs: DotvvmSpaNavigationFailedEventArgs = { 
            ...options, 
            url, 
            serverResponseObject: (err.reason as any).responseObject,
            response: (err.reason as any).response,
            error: err
        };
        events.spaNavigationFailed.trigger(spaNavigationFailedArgs);

        throw err;
    } finally {
        // when no other navigation is running, enable postbacks again
        if (options.postbackId == lastStartedNavigation) {
            gate.enablePostbacks()
        }
    }
}
