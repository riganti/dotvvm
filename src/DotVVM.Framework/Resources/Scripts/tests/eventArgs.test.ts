import { postBack } from "../postback/postback";
import { initDotvvm, watchEvents, getEventHistory } from "./helper";
import { getViewModel, updateViewModelCache } from "../dotvvm-base";
import { DotvvmPostbackError } from "../shared-classes";
import { keys } from "../utils/objects";
import { WrappedResponse } from "../postback/http";

var fetchJson = function<T>(url: string, init: RequestInit): Promise<T> {
    return Promise.resolve({} as T);
}

function appendAdditionalHeaders(headers: Headers, additionalHeaders?: { [key: string]: string }) {
    if (additionalHeaders) {
        for (const key of keys(additionalHeaders)) {
            headers.append(key, additionalHeaders[key]);
        }
    }
}


jest.mock("../postback/http", () => ({
    async fetchCsrfToken() {
        getViewModel().$csrfToken = "test token"
    },

    retryOnInvalidCsrfToken<TResult>(postbackFunction: () => Promise<TResult>) {
        return postbackFunction()
    },

    async getJSON<T>(url: string, spaPlaceHolderUniqueId?: string, additionalHeaders?: { [key: string]: string }): Promise<WrappedResponse<T>> {
        const headers = new Headers();
        headers.append('Accept', 'application/json');
        if (compileConstants.isSpa && spaPlaceHolderUniqueId) {
            headers.append('X-DotVVM-SpaContentPlaceHolder', spaPlaceHolderUniqueId);
        }
        appendAdditionalHeaders(headers, additionalHeaders);

        return { response: { fake: "get" } as any as Response, result: await fetchJson<T>(url, { headers: headers }) };
    },

    async postJSON<T>(url: string, postData: any, additionalHeaders?: { [key: string]: string }): Promise<WrappedResponse<T>> {
        const headers = new Headers();
        headers.append('Content-Type', 'application/json');
        headers.append('X-DotVVM-PostBack', 'true');
        appendAdditionalHeaders(headers, additionalHeaders);

        return { response: { fake: "post" } as any as Response, result: await fetchJson<T>(url, { body: postData, headers: headers, method: "POST" }) };
    }
}));
jest.mock("../postback/gate", () => ({
    isPostbackDisabled(postbackId: number) { return false; },
    enablePostbacks() { },
    disablePostbacks() { }
}));

function validateEvent(actual: { event: string, args: any }, expectedEvent: string, expectedCommandType: PostbackCommandType, ...extraValidations: ((args: any) => void)[]) {
    expect(actual.event).toBe(expectedEvent);

    expect(actual.args.postbackId).toBeGreaterThan(0);
    expect(actual.args.commandType).toBe(expectedCommandType);
    expect(actual.args.args).toBeDefined();
    expect(actual.args.sender).toBeDefined();
    expect(actual.args.viewModel).toBeDefined();

    for (let validation of extraValidations) {
        validation(actual.args);
    }
}

const validations = {
    hasServerResponseObject(args: any) {
        expect(args.serverResponseObject).toBeDefined();
    },
    hasValidationTargetPath(args: any) {
        expect(args.hasValidationTargetPath).toBeDefined();
    },
    hasError(args: any) {
        expect(args.error).toBeInstanceOf(DotvvmPostbackError);
    },
    hasResponse(args: any) {
        expect(args.response).toBeDefined();
    },
    hasHandled(args: any) {
        expect(args.handled).toBeDefined();
    },
    hasCancel(args: any) {
        expect(args.cancel).toBeDefined();
    },
    hasWasInterrupted(args: any) {
        expect(args.wasInterrupted).toBeDefined();
    },
    hasCommandResult(args: any) {
        expect(args.commandResult).toBeDefined();
    },
    hasUrl(args: any) {
        expect(args.url).toBeDefined();
    },
    hasReplace(args: any) {
        expect(args.replace).toBeDefined();
    }
};

const fetchDefinitions = {
    postbackSuccess: async <T>(url: string, init: RequestInit) => {
        return {
            viewModelDiff: {
                Property1: 1
            },
            action: "successfulCommand",
            resources: {},
            updatedControls: {}
        } as any;
    },
    postbackServerError: async <T>(url: string, init: RequestInit) => {
        throw new DotvvmPostbackError({ 
            type: "serverError", 
            status: 500, 
            responseObject: null, 
            response: { fake: "error" } as any as Response 
        });
    },
    networkError: async <T>(url: string, init: RequestInit) => {
        throw new DotvvmPostbackError({ 
            type: "network", 
            err: { fake: "error" } 
        });
    },
    postbackViewModelNotCached: async <T>(url: string, init: RequestInit) => {
        if (JSON.parse(init.body as string).viewModelCacheId) {
            return {
                action: "viewModelNotCached"
            } as any;
        }
        return await fetchDefinitions.postbackSuccess(url, init);
    },
    postbackRedirect: async <T>(url: string, init: RequestInit) => {
        return {
            action: "redirect",
            url: "/newUrl"
        } as any;
    }

};



const originalViewModel = {
    viewModel: {
        Property1: 0,
        Property2: 0
    },
    url: "/myPage",
    virtualDirectory: "",
    renderedResources: ["resource1", "resource2"]
};
initDotvvm(originalViewModel);



test("PostBack + success", async () => {

    fetchJson = fetchDefinitions.postbackSuccess;

    const cleanup = watchEvents(false);
    try {

        await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ]);
        
        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback");
        validateEvent(history[i++], "postbackHandlersCompleted", "postback");
        validateEvent(history[i++], "beforePostback", "postback", validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackViewModelUpdated", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

        expect(history.length).toBe(i);

    }
    finally {
        cleanup();
    }

});

test("PostBack + viewModelCache", async () => {
    var fetchSpy = jest.spyOn(fetchDefinitions, 'postbackViewModelNotCached');

    fetchJson = fetchDefinitions.postbackViewModelNotCached;
    
    const cleanup = watchEvents(false);
    try {

        updateViewModelCache("testId", getViewModel());

        await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ]);
        expect(fetchSpy).toBeCalledTimes(2);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback");
        validateEvent(history[i++], "postbackHandlersCompleted", "postback");
        validateEvent(history[i++], "beforePostback", "postback", validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackViewModelUpdated", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});


test("PostBack + redirect", async () => {
    fetchJson = fetchDefinitions.postbackRedirect;
    
    const cleanup = watchEvents(true);
    try {

        await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ]);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback");
        validateEvent(history[i++], "postbackHandlersCompleted", "postback");
        validateEvent(history[i++], "beforePostback", "postback", validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "redirect", "postback", validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl, validations.hasReplace);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("PostBack + server error", async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});

    fetchJson = fetchDefinitions.postbackServerError;

    const cleanup = watchEvents(false);
    try {

        await expect(postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ])).rejects.toBeInstanceOf(DotvvmPostbackError);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback");
        validateEvent(history[i++], "postbackHandlersCompleted", "postback");
        validateEvent(history[i++], "beforePostback", "postback", validations.hasCancel);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasWasInterrupted, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);
        validateEvent(history[i++], "error", "postback", validations.hasHandled, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("PostBack + network error", async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});
    
    fetchJson = fetchDefinitions.networkError;

    const cleanup = watchEvents(false);
    try {

        await expect(postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ])).rejects.toBeInstanceOf(DotvvmPostbackError);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback");
        validateEvent(history[i++], "postbackHandlersCompleted", "postback");
        validateEvent(history[i++], "beforePostback", "postback", validations.hasCancel);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasWasInterrupted, validations.hasError, validations.hasServerResponseObject);
        validateEvent(history[i++], "error", "postback", validations.hasHandled, validations.hasError, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});