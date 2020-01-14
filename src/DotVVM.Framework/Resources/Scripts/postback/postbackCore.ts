import { serialize } from '../serialization/serialize';
import { deserialize } from '../serialization/deserialize';
import { getViewModel, getInitialUrl } from '../dotvvm-base';
import { loadResourceList, RenderedResourceList, getRenderedResources } from './resourceLoader';
import * as events from '../events';
import { createPostbackArgs } from "../createPostbackArgs";
import * as updater from './updater';
import * as http from './http';
import { DotvvmPostbackError } from '../shared-classes';
import { setIdFragment } from '../utils/dom';
import { handleRedirect } from './redirect';
import * as evaluator from '../utils/evaluator'

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

    return () => http.retryOnInvalidCsrfToken(async () => {
        await http.fetchCsrfToken();

        updateDynamicPathFragments(context, path);

        const postedViewModel = serialize(getViewModel(), {
            pathMatcher: val => context && val == context.$data
        });

        const data = {
            viewModel: postedViewModel,
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

        return processPostbackResponse(options, postedViewModel, result);
    });
}

async function processPostbackResponse(options: PostbackOptions, postedViewModel: any, result: PostbackResponse): Promise<DotvvmAfterPostBackEventArgs> {
    events.postbackCommitInvoked.trigger({});

    processViewModelDiff(result, postedViewModel);

    await loadResourceList(result.resources);

    let isSuccess = false;
    if (result.action == "successfulCommand") {
        updater.updateViewModelAndControls(result, false);
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
    if (typeof id == "object" && id.expr) {
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
