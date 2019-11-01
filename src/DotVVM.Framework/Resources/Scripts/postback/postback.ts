import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { viewModel, currentUrl, renderedResources } from '../dotvvm-root';
import { loadResourceList } from './resourceLoader';
import { events } from '../DotVVM.Events'; 
import * as updater from './updater';
import * as http from './http';

var lastStartedPostback: number;

async function postbackCore(
        options: PostbackOptions, 
        path: string[], 
        command: string, 
        controlUniqueId: string, 
        context: any, 
        commandArgs?: any[]
    ): Promise<PostbackCommitFunction> {

    await http.fetchCsrfToken();

    lastStartedPostback = options.postbackId;

    // perform the postback
    updateDynamicPathFragments(context, path);
    const data = {
        viewModel: serialize(viewModel, { 
            pathMatcher: val => context && val == context.$data 
        }),
        currentPath: path,
        command: command,
        controlUniqueId: processPassedId(controlUniqueId, context),
        additionalData: options.additionalPostbackData,
        renderedResources: renderedResources,
        commandArgs: commandArgs
    };

    try {
        var result = await http.postJSON(
            currentUrl,
            ko.toJSON(data)
        );

        // TODO
        // dotvvm.events.postbackResponseReceived.trigger({});

        return () => processPostbackResponse(options, result);
    }
    catch (err) {

        // if the CSRF token is invalid, retry the postback
        if (err.type === "serverError") {
            if (err.resultObject.action === "invalidCsrfToken") {
                console.log("Resending postback due to invalid CSRF token.") // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                
                viewModel.$csrfToken = null;
                return await postbackCore(options, path, command, controlUniqueId, context, commandArgs);
            }
        }

        throw { 
            ...err, 
            options: options, 
            args: new DotvvmErrorEventArgs(options.sender, viewModel, options.postbackId)
        };
    }
}

async function processPostbackResponse(options: PostbackOptions, result: any): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({});

    const resultObject = parseResultObject(result);

    await loadResourceList(resultObject.resources);
    
    var isSuccess = false;
    if (resultObject.action === "successfulCommand") {
        updater.updateViewModel(() => {

            // remove updated controls
            var updatedControls = updater.cleanUpdatedControls(resultObject);

            // update the viewmodel
            if (resultObject.viewModel) {
                ko.delaySync.pause();
                deserialize(resultObject.viewModel, viewModel);
                ko.delaySync.resume();
            }
            isSuccess = true;

            // remove updated controls which were previously hidden
            updater.cleanUpdatedControls(resultObject, updatedControls);

            // add updated controls
            updater.restoreUpdatedControls(resultObject, updatedControls, true);

        });
        events.postbackViewModelUpdated.trigger({});

    } else if (resultObject.action === "redirect") {
        // redirect
        var promise = this.handleRedirect(resultObject);

        return new DotvvmAfterPostBackWithRedirectEventArgs(options, resultObject, resultObject.commandResult, result, promise);
    }

    var idFragment = resultObject.resultIdFragment;
    if (idFragment) {
        if (location.hash == "#" + idFragment) {
            var element = document.getElementById(idFragment);
            if (element && "function" == typeof element.scrollIntoView) element.scrollIntoView(true);
        }
        else location.hash = idFragment;
    }

    // trigger afterPostback event
    if (!isSuccess) {
        throw new DotvvmErrorEventArgs(options.sender, viewModel, options.postbackId, resultObject);
    } else {
        return new DotvvmAfterPostBackEventArgs(options, resultObject, resultObject.commandResult, result);
    }
} 

function parseResultObject(result: any) {
    // convert classic redirect to DotVVM redirect response
    const locationHeader = result.getResponseHeader("Location");
    if (locationHeader) {
        return { action: "redirect", url: locationHeader };
    }

    const resultObject = JSON.parse(result.responseText);

    // apply viewmodel diff
    if (!resultObject.viewModel && resultObject.viewModelDiff) {
        resultObject.viewModel = updater.patchViewModel(viewModel, resultObject.viewModelDiff);
    }

    return resultObject;
}

function updateDynamicPathFragments(context: any, path: string[]): void {
    for (var i = path.length - 1; i >= 0; i--) {
        if (path[i].indexOf("[$index]") >= 0) {
            path[i] = path[i].replace("[$index]", `[${context.$index()}]`);
        }

        if (path[i].indexOf("[$indexPath]") >= 0) {
            path[i] = path[i].replace("[$indexPath]", `[${context.$indexPath.map(i => i()).join("]/[")}]`);
        }

        context = context.$parentContext;
    }
}

function processPassedId(id: any, context: any): string {
    if (typeof id == "string" || id == null) return id;
    if (typeof id == "object" && id.expr) return this.evaluator.evaluateOnViewModel(context, id.expr);
    throw new Error("invalid argument");
}
