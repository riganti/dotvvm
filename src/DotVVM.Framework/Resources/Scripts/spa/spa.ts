import * as uri from '../utils/uri';
import * as http from '../postback/http';
import { viewModels } from '../DotVVM';

var isSpaReady = ko.observable(false);

export function init(viewModelName: string): void {
    const spaPlaceHolder = getSpaPlaceHolder();
    if (spaPlaceHolder == null) {
        throw "The SpaContentPlaceHolder control was not found!";
    }
    
    window.addEventListener("hashchange", event => handleHashChangeWithHistory(viewModelName, spaPlaceHolder, false));
    handleHashChangeWithHistory(viewModelName, spaPlaceHolder, true);

    window.addEventListener('popstate', event => handlePopState(viewModelName, event, spaPlaceHolder != null));
}

function getSpaPlaceHolder() {
    var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
    if (elements.length == 1) {
        return <HTMLElement>elements[0];
    }
    return null;
}

function handlePopState(viewModelName: string, event: PopStateEvent, inSpaPage: boolean) {
    if (isSpaPage(event.state)) {
        var historyRecord = <HistoryRecord>(event.state);
        if (inSpaPage)
            navigateCore(viewModelName, historyRecord.url);
        else
            dotvvm.performRedirect(historyRecord.url, true);

        event.preventDefault();
    }
}
    
function handleHashChangeWithHistory(viewModelName: string, spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
    if (document.location.hash.indexOf("#!/") === 0) {
        // the user requested navigation to another SPA page
        navigateCore(viewModelName, 
            document.location.hash.substring(2),
            (url) => { replacePage(url); }
        );
    } else {
        var defaultUrl = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
        var containsContent = spaPlaceHolder.hasAttribute("data-dotvvm-spacontentplaceholder-content");

        if (!containsContent && defaultUrl) {
            navigateCore(viewModelName, "/" + defaultUrl, (url) => replacePage(url));
        } else {
            isSpaReady(true);
            spaPlaceHolder.style.display = "";

            var currentRelativeUrl = location.pathname + location.search + location.hash
            replacePage(currentRelativeUrl);
        }
    }
}

export function handleSpaNavigation(element: HTMLElement) {
    var target = element.getAttribute('target');
    if (target == "_blank") {
        return true;
    }
    
    return handleSpaNavigationCore(element.getAttribute('href'));
}

export function handleSpaNavigationCore(url: string | null): Promise<DotvvmNavigationEventArgs> {
    return new Promise<DotvvmNavigationEventArgs>((resolve, reject) => {
        if (url && url.indexOf("/") === 0) {
            var viewModelName = "root"
            url = uri.removeVirtualDirectoryFromUrl(url, viewModelName);
            navigateCore(viewModelName, url, (navigatedUrl) => {
                if (!history.state || history.state.url != navigatedUrl) {
                    pushPage(navigatedUrl);
                }
            }).then(resolve, reject);
        } else {
            reject();
        }
    });
}

function navigateCore(viewModelName: string, url: string, handlePageNavigating?: (url: string) => void): Promise<DotvvmNavigationEventArgs> {
    return new Promise((resolve, reject: (reason?: DotvvmErrorEventArgs) => void) => {

        var viewModel = viewModels[viewModelName].viewModel;

        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();

        // trigger spaNavigating event
        var spaNavigatingArgs = new DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, url);
        this.events.spaNavigating.trigger(spaNavigatingArgs);
        if (spaNavigatingArgs.cancel) {
            return;
        }

        var virtualDirectory = viewModels[viewModelName].virtualDirectory || "";

        // add virtual directory prefix
        var spaUrl = "/___dotvvm-spa___" + uri.addLeadingSlash(url);
        var fullUrl = uri.addLeadingSlash(uri.concatUrl(virtualDirectory, spaUrl));

        // find SPA placeholder
        if (handlePageNavigating) {
            handlePageNavigating(uri.addLeadingSlash(uri.concatUrl(virtualDirectory, uri.addLeadingSlash(url))));
        }

        // send the request
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
        http.getJSON(fullUrl, spaPlaceHolderUniqueId, result => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            var resultObject = JSON.parse(result.responseText);
            this.loadResourceList(resultObject.resources, () => {
                var isSuccess = false;
                if (resultObject.action === "successfulCommand" || !resultObject.action) {
                    try {
                        this.isViewModelUpdating = true;

                        // remove updated controls
                        var updatedControls = this.cleanUpdatedControls(resultObject);

                        // update the viewmodel
                        this.viewModels[viewModelName] = {};
                        for (const p of Object.keys(resultObject)) {
                            this.viewModels[viewModelName][p] = resultObject[p];
                        }

                        ko.delaySync.pause();
                        this.serialization.deserialize(resultObject.viewModel, this.viewModels[viewModelName].viewModel);
                        ko.delaySync.resume();
                        isSuccess = true;

                        // add updated controls
                        this.viewModelObservables[viewModelName](this.viewModels[viewModelName].viewModel);
                        this.restoreUpdatedControls(resultObject, updatedControls, true);

                        this.isSpaReady(true);
                    }
                    finally {
                        this.isViewModelUpdating = false;
                    }
                } else if (resultObject.action === "redirect") {
                    this.handleRedirect(resultObject, viewModelName, true).then(resolve, reject);
                    return;
                }

                // trigger spaNavigated event
                var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(this.viewModels[viewModelName].viewModel, viewModelName, resultObject, result);
                this.events.spaNavigated.trigger(spaNavigatedArgs);
                resolve(spaNavigatedArgs);

                if (!isSuccess && !spaNavigatedArgs.isHandled) {
                    reject();
                    throw "Invalid response from server!";
                }
            });
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            // execute error handlers
            var errArgs = new DotvvmErrorEventArgs(undefined, viewModel, viewModelName, xhr, -1, undefined, true);
            this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
            reject(errArgs);
        });
    });
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
