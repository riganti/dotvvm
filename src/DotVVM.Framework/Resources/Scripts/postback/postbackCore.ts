import { serializeCore } from '../serialization/serialize';
import { getInitialUrl, getViewModelCache, getViewModelCacheId, clearViewModelCache, getState } from '../dotvvm-base';
import { loadResourceList, RenderedResourceList, getRenderedResources } from './resourceLoader';
import * as events from '../events';
import { createPostbackArgs } from "../createPostbackArgs";
import * as updater from './updater';
import * as http from './http';
import { DotvvmPostbackError } from '../shared-classes';
import { setIdFragment } from '../utils/dom';
import { handleRedirect } from './redirect';
import * as evaluator from '../utils/evaluator'
import { isPrimitive } from '../utils/objects';

let lastStartedPostbackId: number;

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

    lastStartedPostbackId = options.postbackId;

    const beforePostbackArgs: DotvvmBeforePostBackEventArgs = {
        ...createPostbackArgs(options),
        cancel: false
    };
    events.beforePostback.trigger(beforePostbackArgs);
    if (beforePostbackArgs.cancel) {
        throw new DotvvmPostbackError({ type: "event", options });
    }

    return await http.retryOnInvalidCsrfToken(async () => {
        await http.fetchCsrfToken();

        updateDynamicPathFragments(context, path);

        const postedViewModel = serializeCore(getState(), {
            pathMatcher: val => context && val == context.$data
        });

        const data: any = {
            currentPath: path,
            command: command,
            controlUniqueId: processPassedId(controlUniqueId, context),
            additionalData: options.additionalPostbackData,
            renderedResources: getRenderedResources(),
            commandArgs: commandArgs
        };

        // if the viewmodel is cached on the server, send only the diff
        if (getViewModelCache()) {
            data.viewModelDiff = updater.diffViewModel(getViewModelCache(), postedViewModel);
            data.viewModelCacheId = getViewModelCacheId();
        } else {
            data.viewModel = postedViewModel;
        }

        const initialUrl = getInitialUrl();
        let result = await http.postJSON<PostbackResponse>(initialUrl, ko.toJSON(data));

        if (result.action == "viewModelNotCached") {
            // repeat the request with full viewmodel
            clearViewModelCache();

            delete data.viewModelCacheId;
            delete data.viewModelCache;
            data.viewModel = postedViewModel;

            result = await http.postJSON<PostbackResponse>(initialUrl, ko.toJSON(data));
        }

        events.postbackResponseReceived.trigger({});

        return async () => {
            try {
                return await processPostbackResponse(options, postedViewModel, result);
            } catch (err) {
                throw new DotvvmPostbackError({ type: "commit", args: { serverResponseObject: err.reason.responseObject, handled: false } });
            }
        };
    });
}

async function processPostbackResponse(options: PostbackOptions, postedViewModel: any, result: PostbackResponse): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({});

    processViewModelDiff(result, postedViewModel);

    await loadResourceList(result.resources);

    let isSuccess = false;
    if (result.action == "successfulCommand") {
        updater.updateViewModelAndControls(result);
        events.postbackViewModelUpdated.trigger({});
        isSuccess = true;
    } else if (result.action == "redirect") {
        // redirect
        const redirectPromise = handleRedirect(result);

        return {
            ...createPostbackArgs(options),
            serverResponseObject: result,
            commandResult: result.commandResult,
            redirectPromise,
            handled: false,
            wasInterrupted: false
        };
    }

    setIdFragment(result.resultIdFragment)

    if (!isSuccess) {
        throw new DotvvmPostbackError({
            type: "serverError",
            responseObject: result
        });
    } else {
        return {
            ...createPostbackArgs(options),
            serverResponseObject: result,
            commandResult: result.commandResult,
            handled: false,
            wasInterrupted: false
        }
    }
}

function processViewModelDiff(result: PostbackResponse, postedViewModel: any) {
    // apply viewmodel diff
    if (!result.viewModel && result.viewModelDiff) {
        result.viewModel = updater.patchViewModel(postedViewModel, result.viewModelDiff);
    }
}

function updateDynamicPathFragments(context: any, path: string[]): void {
    for (let i = path.length - 1; i >= 0; i--) {
        if (path[i].indexOf("[$index]") >= 0) {
            path[i] = path[i].replace("[$index]", `[${context.$index()}]`);
        }

        if (path[i].indexOf("[$indexPath]") >= 0) {
            path[i] = path[i].replace("[$indexPath]", `[${context.$indexPath.map((j: any) => j()).join("]/[")}]`);
        }

        context = context.$parentContext;
    }
}

function processPassedId(id: any, context: any): string {
    if (typeof id == "string" || id == null) {
        return id;
    }
    if (!isPrimitive(id) && id.expr) {
        return evaluator.evaluateOnViewModel(context, id.expr);
    }
    throw new Error("invalid argument");
}

type PostbackResponse =
   (  { viewModel: RootViewModel, viewModelDiff: undefined }
    | { viewModelDiff: object, viewModel: object | undefined })
    & {
        resources?: RenderedResourceList
        commandResult: any
        action: string
        resultIdFragment?: string
    }
