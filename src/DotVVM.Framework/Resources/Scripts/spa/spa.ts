import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import * as events from '../events';
import { navigateCore } from './navigation';
import { DotvvmPostbackError } from '../shared-classes';

export const isSpaReady = ko.observable(false);

export function init(): void {
    const spaPlaceHolder = getSpaPlaceHolder();
    if (spaPlaceHolder == null) {
        throw new Error("The SpaContentPlaceHolder control was not found!");
    }

    window.addEventListener("hashchange", event => handleHashChangeWithHistory(spaPlaceHolder, false));
    handleHashChangeWithHistory(spaPlaceHolder, true);

    window.addEventListener('popstate', event => handlePopState(event, spaPlaceHolder != null));
}

function getSpaPlaceHolder(): HTMLElement | null {
    const elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
    if (elements.length == 1) {
        return <HTMLElement> elements[0];
    }
    return null;
}

export function getSpaPlaceHolderUniqueId(): string {
    return getSpaPlaceHolder()!.getAttribute("data-dotvvm-spacontentplaceholder")!;
}

function handlePopState(event: PopStateEvent, inSpaPage: boolean) {
    if (isSpaPage(event.state)) {
        const historyRecord = <HistoryRecord> (event.state);
        if (inSpaPage) {
            navigateCore(historyRecord.url);
        } else {
            location.replace(historyRecord.url);
        }

        event.preventDefault();
    }
}

function handleHashChangeWithHistory(spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
    if (document.location.hash.indexOf("#!/") === 0) {
        // the user requested navigation to another SPA page
        navigateCore(
            document.location.hash.substring(2),
            (url) => { replacePage(url); }
        );
    } else {
        isSpaReady(true);
        spaPlaceHolder.style.display = "";

        const currentRelativeUrl = location.pathname + location.search + location.hash
        replacePage(currentRelativeUrl);
    }
}

export async function handleSpaNavigation(element: HTMLElement): Promise<DotvvmNavigationEventArgs> {
    const target = element.getAttribute('target');
    if (target == "_blank") {
        return { viewModel: getViewModel(), serverResponseObject: null };
    }

    try {
        return await handleSpaNavigationCore(element.getAttribute('href'));
    } catch (err) {
        // execute error handlers
        var errArgs: DotvvmErrorEventArgs = {
            sender: element, 
            viewModel: getViewModel(),
            handled: false,
            isSpaNavigationError: true,
            serverResponseObject: err
        };
        events.error.trigger(errArgs);
        if (!errArgs.handled) {
            alert("SPA Navigation Error");
        }
        throw err;
    }
}

export async function handleSpaNavigationCore(url: string | null): Promise<DotvvmNavigationEventArgs> {
    if (url && url.indexOf("/") === 0) {
        url = uri.removeVirtualDirectoryFromUrl(url);
        return await navigateCore(url, (navigatedUrl) => {
            if (!history.state || history.state.url != navigatedUrl) {
                pushPage(navigatedUrl);
            }
        });
    } else {
        throw new Error("invalid url");
    }
}

class HistoryRecord {
    constructor(public navigationType: string, public url: string) { }
}

function pushPage(url: string): void {
    history.pushState(new HistoryRecord('SPA', url), '', url);
}

function replacePage(url: string): void {
    history.replaceState(new HistoryRecord('SPA', url), '', url);
}

function isSpaPage(state: any): boolean {
    return state && state.navigationType == 'SPA';
}
