import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import * as events from '../events';
import * as spaEvents from './events';
import { navigateCore } from './navigation';
import * as counter from '../postback/counter';
import { options } from 'knockout';
import { DotvvmPostbackError } from '../shared-classes';
import { logError } from '../utils/logging';

export const isSpaReady = ko.observable(false);

export function init(): void {
    const spaPlaceHolders = getSpaPlaceHolders();
    if (spaPlaceHolders.length == 0) {
        throw new Error("No SpaContentPlaceHolder control was found!");
    }

    window.addEventListener("hashchange", event => handleHashChangeWithHistory(spaPlaceHolders, false));
    handleHashChangeWithHistory(spaPlaceHolders, true);

    window.addEventListener('popstate', event => handlePopState(event, true));
}

function getSpaPlaceHolders(): NodeListOf<HTMLElement> {
    return document.getElementsByName("__dot_SpaContentPlaceHolder");
}

export function getSpaPlaceHoldersUniqueId(): string {
    const spas = Array.from(getSpaPlaceHolders());
    const identifiers = spas.map((element) =>
    {
        return element.getAttribute("data-dotvvm-spacontentplaceholder")?.valueOf()
    });

    return identifiers.join(';');
}

function handlePopState(event: PopStateEvent, inSpaPage: boolean) {
    if (isSpaPage(event.state)) {
        const historyRecord = <HistoryRecord> (event.state);
        if (inSpaPage) {
            handleSpaNavigationCore(historyRecord.url);
        } else {
            location.replace(historyRecord.url);
        }

        event.preventDefault();
    }
}

function handleHashChangeWithHistory(spaPlaceHolders: NodeListOf<HTMLElement>, isInitialPageLoad: boolean) {
    if (document.location.hash.indexOf("#!/") === 0) {
        // the user requested navigation to another SPA page
        handleSpaNavigationCore(
            document.location.hash.substring(2),
            undefined,
            (url) => { replacePage(url); }
        );
    } else {
        isSpaReady(true);
        for (let i = 0; i < spaPlaceHolders.length; i++) {      // IE11 doesn't have forEach on spaPlaceHolders
            spaPlaceHolders[i].style.display = "";
        }

        const currentRelativeUrl = location.pathname + location.search + location.hash
        replacePage(currentRelativeUrl);
    }
}

export async function handleSpaNavigation(element: HTMLElement): Promise<DotvvmNavigationEventArgs | undefined> {
    const target = element.getAttribute('target');
    if (target == "_blank") {
        return;     // TODO: shall we return result if the target is _blank? And what about other targets?
    }

    return await handleSpaNavigationCore(element.getAttribute('href'), element);
}

export async function handleSpaNavigationCore(url: string | null, sender?: HTMLElement, handlePageNavigating?: (url: string) => void): Promise<DotvvmNavigationEventArgs> {

    if (!url || url.indexOf("/") !== 0) {
        throw new Error("Invalid url for SPAN navigation!");
    }

    const currentPostBackCounter = counter.backUpPostBackCounter();

    const options: PostbackOptions = {
        sender,
        commandType: "spaNavigation",
        postbackId: currentPostBackCounter,
        viewModel: getViewModel(),
        args: []
    };

    try {

        url = uri.removeVirtualDirectoryFromUrl(url);
        return await navigateCore(url, options, handlePageNavigating || defaultHandlePageNavigating);

    } catch (err) {

        if (err instanceof DotvvmPostbackError) {
            const commonArgs = {
                ...options,
                serverResponseObject: (err.reason as any).responseObject,
                response: (err.reason as any).response,
                error: err
            }
            // trigger spaNavigationFailed event
            let spaNavigationFailedArgs: DotvvmSpaNavigationFailedEventArgs = { 
                ...commonArgs, 
                url
            };
            spaEvents.spaNavigationFailed.trigger(spaNavigationFailedArgs);

            // execute error handler
            const errArgs: DotvvmErrorEventArgs = {
                ...commonArgs,
                handled: false
            };
            events.error.trigger(errArgs);
            if (!errArgs.handled) {
                logError("spa", "Unexpected exception during SPA navigation", errArgs);
            } else {
                return {
                    ...options,
                    url
                }
            }
        } else {
            logError("spa", "Unexpected exception during SPA navigation", err)
        }

        throw err;
    }
}

function defaultHandlePageNavigating(navigatedUrl: string) {
    if (!history.state || history.state.url != navigatedUrl) {
        pushPage(navigatedUrl);
    }
}

class HistoryRecord {
    constructor(public navigationType: string, public url: string) { }
}

function pushPage(url: string): void {
    // pushState doesn't work when the url is empty
    url = url || "/";
    
    history.pushState(new HistoryRecord('SPA', url), '', url);
}

function replacePage(url: string): void {
    history.replaceState(new HistoryRecord('SPA', url), '', url);
}

function isSpaPage(state: any): boolean {
    return state && state.navigationType == 'SPA';
}
