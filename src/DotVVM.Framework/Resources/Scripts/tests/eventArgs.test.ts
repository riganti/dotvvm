import { postBack, applyPostbackHandlers } from "../postback/postback";
import { initDotvvmWithSpa, watchEvents, getEventHistory } from "./helper";
import { getViewModel, updateViewModelCache, replaceViewModel } from "../dotvvm-base";
import { DotvvmPostbackError } from "../shared-classes";
import { keys } from "../utils/objects";
import { WrappedResponse } from "../postback/http";
import { spaNavigationFailed } from "../spa/events";
import { updateViewModelAndControls } from "../postback/updater";
import { detachAllErrors } from "../validation/error";


jest.unmock("../spa/spa");
const spa = jest.requireActual("../spa/spa");
spa.init = () => {};
spa.getSpaPlaceHolderUniqueId = () => "someId";


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
    expect(actual.args.viewModel).toBeDefined();

    for (let validation of extraValidations) {
        validation(actual.args);
    }
}

const validations = {
    hasSender(args: any) {
        expect(args.sender).toBeDefined();
    },
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
    },
    hasMethodId(args: any) {
        expect(args.methodId).toBeDefined();
    },
    hasMethodArgs(args: any) {
        expect(args.methodArgs).toBeDefined();
    },
    hasArgs(args: any) {
        expect(args.args).toBeDefined();
    },
    hasResult(args: any) {
        expect(args.result).toBeDefined();
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
    postbackValidationErrors: async <T>(url: string, init: RequestInit) => {
        return {
            modelState: [
                {
                    propertyPath: "Property1",
                    errorMessage: "Property 1 is required!"
                }
            ],
            action: "validationErrors"
        } as any;
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
    },

    spaNavigateSuccess: async <T>(url: string, init: RequestInit) => {
        return {
            viewModel: {
                PropertyA: 1,
                PropertyB: 2
            },
            action: "successfulCommand",
            virtualDirectory: "",
            resources: {},
            updatedControls: {
                "c01": "new html"
            }
        } as any;
    },
    spaNavigateRedirect: async <T>(url: string, init: RequestInit) => {
        if (url == "/___dotvvm-spa___/newUrl") {
            return await fetchDefinitions.spaNavigateSuccess(url, init);
        }
        return {
            action: "redirect",
            url: "/newUrl",
            allowSpa: true
        } as any;
    },
    spaNavigateRedirectWithReplace: async <T>(url: string, init: RequestInit) => {
        return {
            action: "redirect",
            url: "/newUrl",
            allowSpa: true,
            replace: true
        } as any;
    },
    spaNavigateError: async <T>(url: string, init: RequestInit) => {
        throw new DotvvmPostbackError({ 
            type: "serverError", 
            status: 500, 
            responseObject: null, 
            response: { fake: "error" } as any as Response 
        });
    },

    staticCommandSuccess: async <T>(url: string, init: RequestInit) => {
        return { 
            type: "successfulCommand", 
            result: 1, 
            response: { fake: "error" } as any as Response 
        } as any; 
    },
    staticCommandServerError: async <T>(url: string, init: RequestInit) => {
        throw new DotvvmPostbackError({ 
            type: "serverError", 
            status: 500, 
            responseObject: null, 
            response: { fake: "error" } as any as Response 
        });
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
initDotvvmWithSpa(originalViewModel);



test("PostBack + success", async () => {

    fetchJson = fetchDefinitions.postbackSuccess;

    const cleanup = watchEvents(false);
    try {

        await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ]);
        
        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackViewModelUpdated", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

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
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackViewModelUpdated", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});


test("PostBack + redirect", async () => {
    fetchJson = fetchDefinitions.postbackRedirect;
    
    const cleanup = watchEvents(false);
    try {

        await postBack(window.document.body, [], "c", "", undefined, [ "concurrency-default" ]);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "redirect", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl, validations.hasReplace);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("PostBack + validation errors", async () => {
    fetchJson = fetchDefinitions.postbackValidationErrors;

    const cleanup = watchEvents(false);
    try {

        await expect(postBack(window.document.body, ["$root"], "c", "", undefined, [ "concurrency-default", [ "validate", { path: "$root" } ] ])).rejects.toBeInstanceOf(DotvvmPostbackError);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "postbackResponseReceived", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "postbackCommitInvoked", "postback", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject);
        validateEvent(history[i++], "validationErrorsChanged", "postback");
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasResponse, validations.hasServerResponseObject);
        
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
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);
        validateEvent(history[i++], "error", "postback", validations.hasSender, validations.hasHandled, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);

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
        validateEvent(history[i++], "postbackHandlersStarted", "postback", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "postback", validations.hasSender);
        validateEvent(history[i++], "beforePostback", "postback", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "afterPostback", "postback", validations.hasSender, validations.hasWasInterrupted, validations.hasError, validations.hasServerResponseObject);
        validateEvent(history[i++], "error", "postback", validations.hasSender, validations.hasHandled, validations.hasError, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});




test("spaNavigation + success", async () => {
    fetchJson = fetchDefinitions.spaNavigateSuccess;

    detachAllErrors();
    
    const cleanup = watchEvents(false);
    try {

        const link = document.createElement("a");
        link.href = "/test";
        await spa.handleSpaNavigation(link, (u: string) => {});

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasSender, validations.hasCancel, validations.hasUrl);
        validateEvent(history[i++], "spaNavigated", "spaNavigation", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("spaNavigation + redirect", async () => {
    fetchJson = fetchDefinitions.spaNavigateRedirect;

    const cleanup = watchEvents(false);
    try {

        const link = document.createElement("a");
        link.href = "/test";
        await spa.handleSpaNavigation(link, (u: string) => {});

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasSender, validations.hasCancel, validations.hasUrl);
        validateEvent(history[i++], "redirect", "spaNavigation", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl, validations.hasReplace);
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasCancel, validations.hasUrl);
        validateEvent(history[i++], "spaNavigated", "spaNavigation", validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();

        replaceViewModel(originalViewModel as RootViewModel);
    }

});

test("spaNavigation + redirect with replace (new page is loaded without SPA)", async () => {
    fetchJson = fetchDefinitions.spaNavigateRedirectWithReplace;

    const cleanup = watchEvents(false);
    try {

        const link = document.createElement("a");
        link.href = "/test";
        await spa.handleSpaNavigation(link, (u: string) => {});

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasSender, validations.hasCancel, validations.hasUrl);
        validateEvent(history[i++], "redirect", "spaNavigation", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject, validations.hasUrl, validations.hasReplace);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();

        replaceViewModel(originalViewModel as RootViewModel);
    }

});

test("spaNavigation + network error", async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});

    fetchJson = fetchDefinitions.networkError;

    const cleanup = watchEvents(false);
    try {

        const link = document.createElement("a");
        link.href = "/test";
        await expect(spa.handleSpaNavigation(link, (u: string) => {})).rejects.toBeInstanceOf(DotvvmPostbackError);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "spaNavigationFailed", "spaNavigation", validations.hasSender, validations.hasError, validations.hasUrl);
        validateEvent(history[i++], "error", "spaNavigation", validations.hasSender, validations.hasHandled, validations.hasError);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("spaNavigation + server error", async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});

    fetchJson = fetchDefinitions.spaNavigateError;

    const cleanup = watchEvents(false);
    try {

        const link = document.createElement("a");
        link.href = "/test";
        await expect(spa.handleSpaNavigation(link, (u: string) => {})).rejects.toBeInstanceOf(DotvvmPostbackError);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "spaNavigating", "spaNavigation", validations.hasSender, validations.hasCancel);
        validateEvent(history[i++], "spaNavigationFailed", "spaNavigation", validations.hasSender, validations.hasResponse, validations.hasServerResponseObject, validations.hasError, validations.hasUrl);
        validateEvent(history[i++], "error", "spaNavigation", validations.hasSender, validations.hasHandled, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});



test("staticCommand (JS only) + success", async () => {
    const cleanup = watchEvents(false);
    try {

        await applyPostbackHandlers(options => (function(a,b) { 
            return Promise.resolve(a.$data.Property1(b)); 
        })(ko.contextFor(window.document.body)), window.document.body, [], [1]);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "staticCommand", validations.hasSender);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("staticCommand (with server call) + success", async () => {
    fetchJson = fetchDefinitions.staticCommandSuccess;

    const cleanup = watchEvents(false);
    try {

        await applyPostbackHandlers(options => (function(a,b){
            return new Promise(function(resolve,reject){
                dotvvm.staticCommandPostback(a,"test",[],options).then(function(r_0){resolve(r_0);},reject);
            });
        }(window.document.body, ko.contextFor(window.document.body))), window.document.body, [], []);
        
        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "staticCommandMethodInvoking", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs);
        validateEvent(history[i++], "staticCommandMethodInvoked", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasResult, validations.hasResponse);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});

test("staticCommand (with two server call) + success", async () => {
    fetchJson = fetchDefinitions.staticCommandSuccess;

    const cleanup = watchEvents(false);
    try {

        await applyPostbackHandlers(options => (function(a,b){
            return new Promise(function(resolve,reject){
                dotvvm.staticCommandPostback(a,"test",[],options).then(function(r_0){
                    dotvvm.staticCommandPostback(a,"test2",[],options).then(function(r_1){
                        resolve(r_1);
                    }, reject);
                }, reject);
            });
        }(window.document.body, ko.contextFor(window.document.body))), window.document.body, [], []);

        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "staticCommandMethodInvoking", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs);
        validateEvent(history[i++], "staticCommandMethodInvoked", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs, validations.hasResult, validations.hasResponse);
        validateEvent(history[i++], "staticCommandMethodInvoking", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs);
        validateEvent(history[i++], "staticCommandMethodInvoked", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs, validations.hasResult, validations.hasResponse);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});


test("staticCommand (with server call) + server error", async () => {
    jest.spyOn(console, 'error').mockImplementation(() => {});

    fetchJson = fetchDefinitions.staticCommandServerError;

    const cleanup = watchEvents(false);
    try {

        await expect(
            applyPostbackHandlers(options => (function(a,b){
                return new Promise(function(resolve,reject){
                    dotvvm.staticCommandPostback(a,"test",[],options).then(function(r_0){resolve(r_0);},reject);
                });
            }(window.document.body, ko.contextFor(window.document.body))), window.document.body, [], [])
        ).rejects.toBeInstanceOf(DotvvmPostbackError);;
        
        var history = getEventHistory();

        let i = 1;  // skip the "init" event
        validateEvent(history[i++], "postbackHandlersStarted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "postbackHandlersCompleted", "staticCommand", validations.hasSender);
        validateEvent(history[i++], "staticCommandMethodInvoking", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs);
        validateEvent(history[i++], "staticCommandMethodFailed", "staticCommand", validations.hasSender, validations.hasMethodId, validations.hasMethodArgs, validations.hasArgs, validations.hasError, validations.hasResponse);
        validateEvent(history[i++], "error", "staticCommand", validations.hasSender, validations.hasHandled, validations.hasResponse, validations.hasError, validations.hasServerResponseObject);

        expect(history.length).toBe(i);
    }
    finally {
        cleanup();
    }

});
