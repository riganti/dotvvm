import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl, getRenderedResources } from '../dotvvm-base';
import { loadResourceList } from './resourceLoader';
import { events, createPostbackArgs } from '../DotVVM.Events'; 
import * as updater from './updater';
import * as http from './http';
import { DotvvmPostbackError } from '../shared-classes';
import { setIdFragment } from '../utils/dom';

var lastStartedPostbackId: number;

export function getLastStartedPostbackId() {
    return lastStartedPostbackId;
}

export async function postbackCore(
        options: PostbackOptions, 
        path: string[], 
        command: string, 
        controlUniqueId: string, 
        context: any, 
        commandArgs?: any[]
    ): Promise<PostbackCommitFunction> {

    return await http.retryOnInvalidCsrfToken(async () => 
    {
        await http.fetchCsrfToken();

        lastStartedPostbackId = options.postbackId;

        updateDynamicPathFragments(context, path);
        const data = {
            viewModel: serialize(getViewModel(), { 
                pathMatcher: val => context && val == context.$data 
            }),
            currentPath: path,
            command: command,
            controlUniqueId: processPassedId(controlUniqueId, context),
            additionalData: options.additionalPostbackData,
            renderedResources: getRenderedResources(),
            commandArgs: commandArgs
        };

        var result = await http.postJSON(
            getInitialUrl(),
            ko.toJSON(data)
        );

        events.postbackResponseReceived.trigger({});

        return () => processPostbackResponse(options, result);
    });
}

async function processPostbackResponse(options: PostbackOptions, result: any): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({});

    const resultObject = parseResultObject(result);

    await loadResourceList(resultObject.resources);
    
    if (resultObject.action === "successfulCommand") {
        updater.updateViewModelAndControls(resultObject, false);
        events.postbackViewModelUpdated.trigger({});

    } else if (resultObject.action === "redirect") {
        // redirect
        var redirectPromise = this.handleRedirect(resultObject);

        return {
            ...createPostbackArgs(options),
            serverResponseObject: resultObject,
            commandResult: resultObject.commandResult,
            xhr: result,
            redirectPromise,
            isHandled: false,
            wasInterrupted: false
        };
    }

    setIdFragment(resultObject.resultIdFragment)

    // trigger afterPostback event
    if (!isSuccess) {
        const error: DotvvmErrorEventArgs = {
            ...createPostbackArgs(options),
            viewModel,
            serverResponseObject: resultObject,
            handled: false
        }
        // TODO: error handling
        throw error;
    } else {
        return {
            ...createPostbackArgs(options),
            serverResponseObject: resultObject,
            commandResult: resultObject.commandResult,
            xhr: result,
            redirectPromise,
            isHandled: false,
            wasInterrupted: false
        }
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
