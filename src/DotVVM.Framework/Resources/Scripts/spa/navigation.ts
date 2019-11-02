import * as postback from '../postback/postback';
import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import { DotvvmPostbackError } from '../shared-classes';
import { loadResourceList } from '../postback/resourceLoader';
import * as updater from '../postback/updater';
import * as counter from '../postback/counter';
import { events } from '../DotVVM.Events';
import { getSpaPlaceHolderUniqueId, isSpaReady } from './spa';

export async function navigateCore(url: string, handlePageNavigating?: (url: string) => void): Promise<void> {
    
    await http.retryOnInvalidCsrfToken(async () => {

        // prevent double postbacks
        var currentPostBackCounter = counter.backUpPostBackCounter();

        // trigger spaNavigating event
        var spaNavigatingArgs : DotvvmSpaNavigatingEventArgs = {
            getViewModel(),
            newUrl: url,
            cancel: false
        };
        events.spaNavigating.trigger(spaNavigatingArgs);
        if (spaNavigatingArgs.cancel) {
            throw new DotvvmPostbackError({ type: "event" });
        }

        // compose URLs
        var spaFullUrl = uri.addVirtualDirectoryToUrl("/___dotvvm-spa___" + uri.addLeadingSlash(url));
        var displayUrl = uri.addVirtualDirectoryToUrl(url);

        // use custom browser navigation function
        if (handlePageNavigating) {
            handlePageNavigating(displayUrl);
        }

        // send the request
        var resultObject = await http.getJSON(spaFullUrl, getSpaPlaceHolderUniqueId());

        // if another postback has already been passed, don't do anything
        if (!counter.isPostBackStillActive(currentPostBackCounter)) {
            return;
        }

        await loadResourceList(resultObject.resources);

        if (resultObject.action === "successfulCommand" || !resultObject.action) {
            updater.updateViewModelAndControls(resultObject, true);
            isSpaReady(true);
        } else if (resultObject.action === "redirect") {
            return await handleRedirect(resultObject, true);
        }

        // trigger spaNavigated event
        var spaNavigatedArgs: DotvvmSpaNavigatedEventArgs = {
            viewModel: getViewModel(),
            serverResponseObject: resultObject,
            isSpa: true,
            isHandled: true
        };
        events.spaNavigated.trigger(spaNavigatedArgs);
    });
    
    // // if another postback has already been passed, don't do anything
    // if (!this.isPostBackStillActive(currentPostBackCounter)) return;

    // // execute error handlers
    // var errArgs = new DotvvmErrorEventArgs(undefined, viewModel, viewModelName, xhr, -1, undefined, true);
    // this.events.error.trigger(errArgs);
    // if (!errArgs.handled) {
    //     alert(xhr.responseText);
    // }
    // reject(errArgs);
}
