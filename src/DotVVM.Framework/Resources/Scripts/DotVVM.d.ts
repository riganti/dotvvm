declare class DotvvmDomUtils {
    onDocumentReady(callback: () => void): void;
    attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture?: boolean): void;
}
declare class HistoryRecord {
    navigationType: string;
    url: string;
    constructor(navigationType: string, url: string);
}
declare class DotvvmSpaHistory {
    pushPage(url: string): void;
    replacePage(url: string): void;
    isSpaPage(state: any): boolean;
    getHistoryRecord(state: any): HistoryRecord;
}
declare class DotvvmEvents {
    init: DotvvmEvent<DotvvmEventArgs>;
    beforePostback: DotvvmEvent<DotvvmBeforePostBackEventArgs>;
    afterPostback: DotvvmEvent<DotvvmAfterPostBackEventArgs>;
    error: DotvvmEvent<DotvvmErrorEventArgs>;
    spaNavigating: DotvvmEvent<DotvvmSpaNavigatingEventArgs>;
    spaNavigated: DotvvmEvent<DotvvmSpaNavigatedEventArgs>;
    redirect: DotvvmEvent<DotvvmRedirectEventArgs>;
    postbackHandlersStarted: DotvvmEvent<{}>;
    postbackHandlersCompleted: DotvvmEvent<{}>;
    postbackResponseReceived: DotvvmEvent<{}>;
    postbackCommitInvoked: DotvvmEvent<{}>;
    postbackViewModelUpdated: DotvvmEvent<{}>;
    postbackRejected: DotvvmEvent<{}>;
    staticCommandMethodInvoking: DotvvmEvent<{
        args: any[];
        command: string;
    }>;
    staticCommandMethodInvoked: DotvvmEvent<{
        args: any[];
        command: string;
        result: any;
        xhr: XMLHttpRequest;
    }>;
    staticCommandMethodFailed: DotvvmEvent<{
        args: any[];
        command: string;
        xhr: XMLHttpRequest;
        error?: any;
    }>;
}
declare class DotvvmEventHandler<T> {
    handler: (f: T) => void;
    isOneTime: boolean;
    constructor(handler: (f: T) => void, isOneTime: boolean);
}
declare class DotvvmEvent<T> {
    readonly name: string;
    private readonly triggerMissedEventsOnSubscribe;
    private handlers;
    private history;
    constructor(name: string, triggerMissedEventsOnSubscribe?: boolean);
    subscribe(handler: (data: T) => void): void;
    subscribeOnce(handler: (data: T) => void): void;
    unsubscribe(handler: (data: T) => void): void;
    trigger(data: T): void;
}
interface PostbackEventArgs extends DotvvmEventArgs {
    postbackClientId: number;
    viewModelName: string;
    sender?: Element;
    xhr?: XMLHttpRequest;
    serverResponseObject?: any;
}
interface DotvvmEventArgs {
    viewModel: any;
}
declare class DotvvmErrorEventArgs implements PostbackEventArgs {
    sender: Element | undefined;
    viewModel: any;
    viewModelName: any;
    xhr: XMLHttpRequest;
    postbackClientId: any;
    serverResponseObject: any;
    isSpaNavigationError: boolean;
    handled: boolean;
    constructor(sender: Element | undefined, viewModel: any, viewModelName: any, xhr: XMLHttpRequest, postbackClientId: any, serverResponseObject?: any, isSpaNavigationError?: boolean);
}
declare class DotvvmBeforePostBackEventArgs implements PostbackEventArgs {
    sender: HTMLElement;
    viewModel: any;
    viewModelName: string;
    postbackClientId: number;
    cancel: boolean;
    clientValidationFailed: boolean;
    constructor(sender: HTMLElement, viewModel: any, viewModelName: string, postbackClientId: number);
}
declare class DotvvmAfterPostBackEventArgs implements PostbackEventArgs {
    postbackOptions: PostbackOptions;
    serverResponseObject: any;
    commandResult: any;
    xhr?: XMLHttpRequest | undefined;
    isHandled: boolean;
    wasInterrupted: boolean;
    readonly postbackClientId: number;
    readonly viewModelName: string;
    readonly viewModel: any;
    readonly sender: HTMLElement | undefined;
    constructor(postbackOptions: PostbackOptions, serverResponseObject: any, commandResult?: any, xhr?: XMLHttpRequest | undefined);
}
declare class DotvvmSpaNavigatingEventArgs implements DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    newUrl: string;
    cancel: boolean;
    constructor(viewModel: any, viewModelName: string, newUrl: string);
}
declare class DotvvmSpaNavigatedEventArgs implements DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    serverResponseObject: any;
    xhr?: XMLHttpRequest | undefined;
    isHandled: boolean;
    constructor(viewModel: any, viewModelName: string, serverResponseObject: any, xhr?: XMLHttpRequest | undefined);
}
declare class DotvvmRedirectEventArgs implements DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    url: string;
    replace: boolean;
    isHandled: boolean;
    constructor(viewModel: any, viewModelName: string, url: string, replace: boolean);
}
declare class DotvvmFileUpload {
    showUploadDialog(sender: HTMLElement): void;
    private getIframe;
    private openUploadDialog;
    createUploadId(sender: HTMLElement, iframe: HTMLElement): void;
    reportProgress(targetControlId: any, isBusy: boolean, progress: number, result: DotvvmFileUploadData[] | string): void;
}
declare class DotvvmFileUploadCollection {
    Files: KnockoutObservableArray<KnockoutObservable<DotvvmFileUpload>>;
    Progress: KnockoutObservable<number>;
    Error: KnockoutObservable<string>;
    IsBusy: KnockoutObservable<boolean>;
}
declare class DotvvmFileUploadData {
    FileId: KnockoutObservable<string>;
    FileName: KnockoutObservable<string>;
    FileSize: KnockoutObservable<DotvvmFileSize>;
    IsFileTypeAllowed: KnockoutObservable<boolean>;
    IsMaxSizeExceeded: KnockoutObservable<boolean>;
    IsAllowed: KnockoutObservable<boolean>;
}
declare class DotvvmFileSize {
    Bytes: KnockoutObservable<number>;
    FormattedText: KnockoutObservable<string>;
}
declare class DotvvmGlobalize {
    private getGlobalize;
    format(format: string, ...values: any[]): string;
    formatString(format: string, value: any): any;
    parseDotvvmDate(value: string): Date | null;
    parseNumber(value: string): number;
    parseDate(value: string, format: string, previousValue?: Date): any;
    bindingDateToString(value: KnockoutObservable<string | Date> | string | Date, format?: string): "" | KnockoutComputed<any>;
    bindingNumberToString(value: KnockoutObservable<string | number> | string | number, format?: string): "" | KnockoutComputed<any>;
}
declare type DotvvmPostbackHandler = {
    execute(callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions): Promise<PostbackCommitFunction>;
    name?: string;
    after?: (string | DotvvmPostbackHandler)[];
    before?: (string | DotvvmPostbackHandler)[];
};
declare type PostbackCommitFunction = () => Promise<DotvvmAfterPostBackEventArgs>;
declare type PostbackRejectionReason = {
    type: "handler";
    handler: DotvvmPostbackHandler;
    message?: string;
} | {
    type: 'network';
    args: DotvvmErrorEventArgs;
} | {
    type: 'commit';
    args: DotvvmErrorEventArgs;
} | {
    type: 'event';
} & {
    options?: PostbackOptions;
};
interface AdditionalPostbackData {
    [key: string]: any;
    validationTargetPath?: string;
}
declare class PostbackOptions {
    readonly postbackId: number;
    readonly sender?: HTMLElement | undefined;
    readonly args: any[];
    readonly viewModel?: any;
    readonly viewModelName?: string | undefined;
    readonly additionalPostbackData: AdditionalPostbackData;
    constructor(postbackId: number, sender?: HTMLElement | undefined, args?: any[], viewModel?: any, viewModelName?: string | undefined);
}
declare class ConfirmPostBackHandler implements DotvvmPostbackHandler {
    message: string;
    constructor(message: string);
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T>;
}
declare class SuppressPostBackHandler implements DotvvmPostbackHandler {
    suppress: any;
    constructor(suppress: any);
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T>;
}
declare type DotvvmPostBackHandlerConfiguration = {
    name: string;
    options: (context: KnockoutBindingContext) => any;
};
declare type ClientFriendlyPostbackHandlerConfiguration = string | DotvvmPostbackHandler | DotvvmPostBackHandlerConfiguration | [string, object] | [string, (context: KnockoutBindingContext, data: any) => any];
interface ISerializationOptions {
    serializeAll?: boolean;
    oneLevel?: boolean;
    ignoreSpecialProperties?: boolean;
    pathMatcher?: (vm: any) => boolean;
    path?: string[];
    pathOnly?: boolean;
    restApiTarget?: boolean;
}
declare class DotvvmSerialization {
    deserialize(viewModel: any, target?: any, deserializeAll?: boolean): any;
    wrapObservable<T>(obj: T): KnockoutObservable<T>;
    serialize(viewModel: any, opt?: ISerializationOptions): any;
    validateType(value: any, type: string): boolean;
    private findObject;
    flatSerialize(viewModel: any): any;
    getPureObject(viewModel: any): {};
    private pad;
    serializeDate(date: string | Date | null, convertToUtc?: boolean): string | null;
}
declare class DotvvmValidationContext {
    valueToValidate: any;
    parentViewModel: any;
    parameters: any[];
    constructor(valueToValidate: any, parentViewModel: any, parameters: any[]);
}
declare class DotvvmValidationObservableMetadata {
    elementsMetadata: DotvvmValidationElementMetadata[];
}
declare class DotvvmValidationElementMetadata {
    element: HTMLElement;
    dataType: string;
    format: string;
    domNodeDisposal: boolean;
    elementValidationState: boolean;
}
declare class DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean;
    isEmpty(value: string): boolean;
    getValidationMetadata(property: KnockoutObservable<any>): DotvvmValidationObservableMetadata;
}
declare class DotvvmRequiredValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext): boolean;
}
declare class DotvvmRegularExpressionValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext): boolean;
}
declare class DotvvmIntRangeValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext): boolean;
}
declare class DotvvmEnforceClientFormatValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean;
}
declare class DotvvmRangeValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean;
}
declare class DotvvmNotNullValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext): boolean;
}
declare class DotvvmEmailAddressValidator extends DotvvmValidatorBase {
    isValid(context: DotvvmValidationContext): boolean;
}
declare type KnockoutValidatedObservable<T> = KnockoutObservable<T> & {
    validationErrors?: KnockoutObservableArray<ValidationError>;
};
declare class ValidationError {
    validatedObservable: KnockoutValidatedObservable<any>;
    errorMessage: string;
    constructor(validatedObservable: KnockoutValidatedObservable<any>, errorMessage: string);
    static getOrCreate(validatedObservable: KnockoutValidatedObservable<any> & {
        wrappedProperty?: any;
    }): KnockoutObservableArray<ValidationError>;
    static isValid(validatedObservable: KnockoutValidatedObservable<any>): boolean;
    clear(validation: DotvvmValidation): void;
}
interface IDotvvmViewModelInfo {
    validationRules?: {
        [typeName: string]: {
            [propertyName: string]: IDotvvmPropertyValidationRuleInfo[];
        };
    };
}
interface IDotvvmPropertyValidationRuleInfo {
    ruleName: string;
    errorMessage: string;
    parameters: any[];
}
declare type DotvvmValidationRules = {
    [name: string]: DotvvmValidatorBase;
};
declare type DotvvmValidationElementUpdateFunctions = {
    [name: string]: (element: HTMLElement, errorMessages: string[], param: any) => void;
};
declare class DotvvmValidation {
    rules: DotvvmValidationRules;
    errors: KnockoutObservableArray<ValidationError>;
    events: {
        validationErrorsChanged: DotvvmEvent<DotvvmEventArgs>;
    };
    elementUpdateFunctions: DotvvmValidationElementUpdateFunctions;
    constructor(dotvvm: DotVVM);
    /**
     * Validates the specified view model
    */
    validateViewModel(viewModel: any): void;
    validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, rulesForProperty: IDotvvmPropertyValidationRuleInfo[]): void;
    mergeValidationRules(args: DotvvmAfterPostBackEventArgs): void;
    /**
      * Clears validation errors from the passed viewModel, from its children
      * and from the DotvvmValidation.errors array
    */
    clearValidationErrors(validatedObservable: KnockoutValidatedObservable<any>): void;
    /**
     * Gets validation errors from the passed object and its children.
     * @param target Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @param includeErrorsFromChildren Sets whether to include errors from children at all
     * @returns By default returns only errors from the viewModel's immediate children
     */
    getValidationErrors(validationTargetObservable: KnockoutValidatedObservable<any>, includeErrorsFromGrandChildren: any, includeErrorsFromTarget: any, includeErrorsFromChildren?: boolean): ValidationError[];
    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs): void;
    private addValidationError;
}
declare var dotvvm: DotVVM;
interface Document {
    getElementByDotvvmId(id: string): HTMLElement;
}
interface IRenderedResourceList {
    [name: string]: string;
}
interface IDotvvmPostbackScriptFunction {
    (pageArea: string, sender: HTMLElement, pathFragments: string[], controlId: string, useWindowSetTimeout: boolean, validationTarget: string, context: any, handlers: DotvvmPostBackHandlerConfiguration[]): void;
}
interface IDotvvmExtensions {
}
interface IDotvvmViewModelInfo {
    viewModel?: any;
    renderedResources?: string[];
    url?: string;
    virtualDirectory?: string;
}
interface IDotvvmViewModels {
    [name: string]: IDotvvmViewModelInfo;
}
interface IDotvvmPostbackHandlerCollection {
    [name: string]: ((options: any) => DotvvmPostbackHandler);
    confirm: (options: {
        message?: string;
    }) => ConfirmPostBackHandler;
    suppress: (options: {
        suppress?: boolean;
    }) => SuppressPostBackHandler;
}
declare class DotVVM {
    private postBackCounter;
    private lastStartedPostack;
    private fakeRedirectAnchor;
    private resourceSigns;
    private isViewModelUpdating;
    private spaHistory;
    viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    };
    isSpaReady: KnockoutObservable<boolean>;
    viewModels: IDotvvmViewModels;
    culture: string;
    serialization: DotvvmSerialization;
    postbackHandlers: IDotvvmPostbackHandlerCollection;
    private suppressOnDisabledElementHandler;
    private beforePostbackEventPostbackHandler;
    private isPostBackRunningHandler;
    private createWindowSetTimeoutHandler;
    private windowSetTimeoutHandler;
    private commonConcurrencyHandler;
    private defaultConcurrencyPostbackHandler;
    private postbackQueues;
    getPostbackQueue(name?: string): {
        queue: (() => void)[];
        noRunning: number;
    };
    private postbackHandlersStartedEventHandler;
    private postbackHandlersCompletedEventHandler;
    globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[];
    globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[];
    events: DotvvmEvents;
    globalize: DotvvmGlobalize;
    evaluator: DotvvmEvaluator;
    domUtils: DotvvmDomUtils;
    fileUpload: DotvvmFileUpload;
    validation: DotvvmValidation;
    extensions: IDotvvmExtensions;
    useHistoryApiSpaNavigation: boolean;
    isPostbackRunning: KnockoutObservable<boolean>;
    init(viewModelName: string, culture: string): void;
    private handlePopState;
    private handleHashChangeWithHistory;
    private handleHashChange;
    private persistViewModel;
    private backUpPostBackConter;
    private isPostBackStillActive;
    staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback?: (_: any) => void, errorCallback?: (xhr: XMLHttpRequest, error?: any) => void): void;
    private processPassedId;
    protected getPostbackHandler(name: string): (options: any) => DotvvmPostbackHandler;
    private isPostbackHandler;
    findPostbackHandlers(knockoutContext: any, config: ClientFriendlyPostbackHandlerConfiguration[]): DotvvmPostbackHandler[];
    private sortHandlers;
    private applyPostbackHandlersCore;
    applyPostbackHandlers(callback: (options: PostbackOptions) => Promise<PostbackCommitFunction | undefined>, sender: HTMLElement, handlers?: ClientFriendlyPostbackHandlerConfiguration[], args?: any[], context?: any, viewModel?: any, viewModelName?: string): Promise<DotvvmAfterPostBackEventArgs>;
    postbackCore(options: PostbackOptions, path: string[], command: string, controlUniqueId: string, context: any, commandArgs?: any[]): Promise<() => Promise<DotvvmAfterPostBackEventArgs>>;
    handleSpaNavigation(element: HTMLElement): boolean;
    handleSpaNavigationCore(url: string | null): boolean;
    postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, context?: any, handlers?: ClientFriendlyPostbackHandlerConfiguration[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs>;
    private loadResourceList;
    private loadResourceElements;
    private getSpaPlaceHolder;
    private navigateCore;
    private handleRedirect;
    private performRedirect;
    private fixSpaUrlPrefix;
    private removeVirtualDirectoryFromUrl;
    private addLeadingSlash;
    private concatUrl;
    patch(source: any, patch: any): any;
    private updateDynamicPathFragments;
    private postJSON;
    private getJSON;
    getXHR(): XMLHttpRequest;
    private cleanUpdatedControls;
    private restoreUpdatedControls;
    unwrapArrayExtension(array: any): any;
    buildRouteUrl(routePath: string, params: any): string;
    buildUrlSuffix(urlSuffix: string, query: any): string;
    private isPostBackProhibited;
    private addKnockoutBindingHandlers;
}
declare class DotvvmEvaluator {
    evaluateOnViewModel(context: any, expression: any): any;
    evaluateOnContext(context: any, expression: string): any;
    getDataSourceItems(viewModel: any): any;
    tryEval(func: () => any): any;
    isObservableArray(instance: any): instance is KnockoutObservableArray<any>;
    wrapObservable(func: () => any, isArray?: boolean): KnockoutComputed<any>;
    private updateObservable;
    private updateObservableArray;
    private getExpressionResult;
}
declare type ApiComputed<T> = KnockoutObservable<T | null> & {
    refreshValue: (throwOnError?: boolean) => PromiseLike<any> | undefined;
};
declare type Result<T> = {
    type: 'error';
    error: any;
} | {
    type: 'result';
    result: T;
};
interface DotVVM {
    invokeApiFn<T>(callback: () => PromiseLike<T>): ApiComputed<T>;
    apiRefreshOn<T>(value: KnockoutObservable<T>, refreshOn: KnockoutObservable<any>): KnockoutObservable<T>;
    apiStore<T>(value: KnockoutObservable<T>, targetProperty: KnockoutObservable<any>): KnockoutObservable<T>;
    api: {
        [name: string]: any;
    };
    eventHub: DotvvmEventHub;
}
declare class DotvvmEventHub {
    private map;
    notify(id: string): void;
    get(id: string): KnockoutObservable<number>;
}
declare function basicAuthenticatedFetch(input: RequestInfo, init: RequestInit): any;
