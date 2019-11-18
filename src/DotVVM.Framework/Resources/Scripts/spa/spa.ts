import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { getViewModel } from '../dotvvm-base';
import { events } from '../DotVVM.Events';
import { navigateCore } from './navigation';
import { DotvvmPostbackError } from '../shared-classes';
import { performRedirect } from '../postback/redirect';

export const isSpaReady = ko.observable(false);


export function init(viewModelName: string): void {
    const spaPlaceHolder = getSpaPlaceHolder();
    if (spaPlaceHolder == null) {
        throw "The SpaContentPlaceHolder control was not found!";
    }

    window.addEventListener("hashchange", event => handleHashChangeWithHistory(viewModelName, spaPlaceHolder, false));
    handleHashChangeWithHistory(viewModelName, spaPlaceHolder, true);

    window.addEventListener('popstate', event => handlePopState(event, spaPlaceHolder != null));
}

function getSpaPlaceHolder(): HTMLElement | null {
    var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
    if (elements.length == 1) {
        return <HTMLElement>elements[0];
    }
    return null;
}

export function getSpaPlaceHolderUniqueId(): string {
    return getSpaPlaceHolder()!.getAttribute("data-dotvvm-spacontentplaceholder")!;
}

function handlePopState(event: PopStateEvent, inSpaPage: boolean) {
    if (isSpaPage(event.state)) {
        var historyRecord = <HistoryRecord>(event.state);
        if (inSpaPage)
            navigateCore(historyRecord.url);
        else
            performRedirect(historyRecord.url, true, false);

        event.preventDefault();
    }
}

function handleHashChangeWithHistory(viewModelName: string, spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
    if (document.location.hash.indexOf("#!/") === 0) {
        // the user requested navigation to another SPA page
        navigateCore(
            document.location.hash.substring(2),
            (url) => { replacePage(url); }
        );
    } else {
        var defaultUrl = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
        var containsContent = spaPlaceHolder.hasAttribute("data-dotvvm-spacontentplaceholder-content");

        if (!containsContent && defaultUrl) {
            navigateCore("/" + defaultUrl, (url) => replacePage(url));
        } else {
            isSpaReady(true);
            spaPlaceHolder.style.display = "";

            var currentRelativeUrl = location.pathname + location.search + location.hash
            replacePage(currentRelativeUrl);
        }
    }
}

export function handleSpaNavigation(element: HTMLElement): Promise<DotvvmNavigationEventArgs> {
    var target = element.getAttribute('target');
    if (target == "_blank") {
        return true; // TODO: https://github.com/riganti/dotvvm/pull/787/files#diff-edefee5e25549b2a6ed0136e520e009fL662
    }

    return handleSpaNavigationCore(element.getAttribute('href'));
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
