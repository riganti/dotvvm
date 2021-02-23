import { serializeCore } from '../serialization/serialize';
import { getInitialUrl, getViewModelCache, getViewModelCacheId, clearViewModelCache, getState } from '../dotvvm-base';
import { loadResourceList, RenderedResourceList, getRenderedResources } from './resourceLoader';
import * as events from '../events';
import * as updater from './updater';
import * as http from './http';
import { setIdFragment } from '../utils/dom';
import { handleRedirect } from './redirect';
import * as evaluator from '../utils/evaluator'
import * as gate from './gate'
import { showValidationErrorsFromServer } from '../validation/validation';
import { DotvvmPostbackError } from '../shared-classes';
import { getKnownTypes, updateTypeInfo } from '../metadata/typeMap';
import { isPrimitive } from '../utils/objects';
import * as stateManager from '../state-manager'

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
        ...options,
        cancel: false
    };
    events.beforePostback.trigger(beforePostbackArgs);
    if (beforePostbackArgs.cancel) {
        throw new DotvvmPostbackError({ type: "event" });
    }

    return await http.retryOnInvalidCsrfToken(async () => {
        await http.fetchCsrfToken();

        updateDynamicPathFragments(context, path);

        const initialState = getState()
        const postedViewModel = serializeCore(initialState, {
            pathMatcher: val => context && val == context.$data[stateManager.currentStateSymbol]
        });

        const data: any = {
            currentPath: path,
            command: command,
            controlUniqueId: processPassedId(controlUniqueId, context),
            validationTargetPath: options.validationTargetPath,
            renderedResources: getRenderedResources(),
            commandArgs: commandArgs,
            knownTypeMetadata: getKnownTypes()
        };

        // if the viewmodel is cached on the server, send only the diff
        if (getViewModelCache()) {
            data.viewModelDiff = updater.diffViewModel(getViewModelCache(), postedViewModel);
            data.viewModelCacheId = getViewModelCacheId();
        } else {
            data.viewModel = postedViewModel;
        }

        const initialUrl = getInitialUrl();
        let response = await http.postJSON<PostbackResponse>(initialUrl, JSON.stringify(data));

        if (response.result.action == "viewModelNotCached") {
            // repeat the request with full viewmodel
            clearViewModelCache();

            delete data.viewModelCacheId;
            delete data.viewModelCache;
            data.viewModel = postedViewModel;

            response = await http.postJSON<PostbackResponse>(initialUrl, JSON.stringify(data));
        }

        events.postbackResponseReceived.trigger({
            ...options,
            response: response.response!,
            serverResponseObject: response.result
        });

        return async () => {
            try {
                return await processPostbackResponse(options, context, postedViewModel, initialState, response.result, response.response!);
            } catch (err) {
                if (err instanceof DotvvmPostbackError) {
                    throw err;
                }
                
                throw new DotvvmPostbackError({ 
                    type: "commit", 
                    args: { 
                        ...options, 
                        serverResponseObject: response.result, 
                        response: response.response,
                        handled: false, 
                        error: err 
                    } 
                });
            }
        };
    });
}

async function processPostbackResponse(options: PostbackOptions, context: any, postedViewModel: any, initialState: any, result: PostbackResponse, response: Response): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({
        ...options,
        response,
        serverResponseObject: result
    });

    processViewModelDiff(result, postedViewModel);

    await loadResourceList(result.resources);

    if (gate.isPostbackDisabled(options.postbackId))
        throw "Postbacks are disabled"

    let isSuccess = false;
    if (result.action == "successfulCommand") {
        updateTypeInfo(result.typeMetadata)
        result.viewModel = updater.patchViewModel(getState(), result.viewModel)
        updater.updateViewModelAndControls(result);
        events.postbackViewModelUpdated.trigger({
            ...options,
            response,
            serverResponseObject: result
        });
        isSuccess = true;
    } else if (result.action == "redirect") {
        await handleRedirect(options, result, response);

        return {
            ...options,
            response,
            serverResponseObject: result,
            commandResult: result.commandResult,
            wasInterrupted: false
        };
    } else if (result.action == "validationErrors") {
        showValidationErrorsFromServer(context, options.validationTargetPath!, result, options);
        throw new DotvvmPostbackError({
            type: "validation",
            response,
            responseObject: result
        });
    }

    setIdFragment(result.resultIdFragment)

    if (!isSuccess) {
        throw new DotvvmPostbackError({
            type: "serverError",
            response,
            responseObject: result
        });
    } else {
        return {
            ...options,
            response,
            serverResponseObject: result,
            commandResult: result.commandResult,
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
        resultIdFragment?: string,
        typeMetadata?: TypeMap
        customData?: { [key: string]: any }
    }
