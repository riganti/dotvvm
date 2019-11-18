import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl, getRenderedResources } from '../dotvvm-base';
import { loadResourceList, RenderedResourceList } from './resourceLoader';
import { events, createPostbackArgs } from '../DotVVM.Events'; 
import * as updater from './updater';
import * as http from './http';
import { DotvvmPostbackError } from '../shared-classes';
import { setIdFragment } from '../utils/dom';
import { handleRedirect } from './redirect';
import * as evaluator from '../DotVVM.Evaluator'

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

        const result = await http.postJSON<PostbackResponse>(
            getInitialUrl(),
            ko.toJSON(data)
        );

        events.postbackResponseReceived.trigger({});

        return () => processPostbackResponse(options, result);
    });
}

async function processPostbackResponse(options: PostbackOptions, result: PostbackResponse): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({});

    processViewModelDiff(result);

    await loadResourceList(result.resources);
    
    if (result.action == "successfulCommand") {
        updater.updateViewModelAndControls(result, false);
        events.postbackViewModelUpdated.trigger({});

    } else if (result.action == "redirect") {
        // redirect
        var redirectPromise = handleRedirect(result);

        return {
            ...createPostbackArgs(options),
            serverResponseObject: result,
            commandResult: result.commandResult,
            xhr: result,
            redirectPromise,
            isHandled: false,
            wasInterrupted: false
        };
    }

    setIdFragment(result.resultIdFragment)

    // trigger afterPostback event
    if (!isSuccess) {
        const error: DotvvmErrorEventArgs = {
            ...createPostbackArgs(options),
            viewModel,
            serverResponseObject: result,
            handled: false
        }
        // TODO: error handling
        throw error;
    } else {
        return {
            ...createPostbackArgs(options),
            serverResponseObject: result,
            commandResult: result.commandResult,
            xhr: result,
            isHandled: false,
            wasInterrupted: false
        }
    }
}

function processViewModelDiff(result: PostbackResponse) {
    // apply viewmodel diff
    if (!result.viewModel && result.viewModelDiff) {
        result.viewModel = updater.patchViewModel(getViewModel(), result.viewModelDiff);
    }
}

function updateDynamicPathFragments(context: any, path: string[]): void {
    for (var i = path.length - 1; i >= 0; i--) {
        if (path[i].indexOf("[$index]") >= 0) {
            path[i] = path[i].replace("[$index]", `[${context.$index()}]`);
        }

        if (path[i].indexOf("[$indexPath]") >= 0) {
            path[i] = path[i].replace("[$indexPath]", `[${context.$indexPath.map((i: any) => i()).join("]/[")}]`);
        }

        context = context.$parentContext;
    }
}

function processPassedId(id: any, context: any): string {
    if (typeof id == "string" || id == null) return id;
    if (typeof id == "object" && id.expr) return evaluator.evaluateOnViewModel(context, id.expr);
    throw new Error("invalid argument");
}

type PostbackResponse =
   (  { viewModel: RootViewModel, viewModelDiff: undefined }
    | { viewModelDiff: object, viewModel: object | undefined })
    & {
        resources: RenderedResourceList
        commandResult: any
        action: string
        resultIdFragment?: string
    }
