/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
declare class DotvvmDomUtils {
    onDocumentReady(callback: () => void): void;
    attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture?: boolean): void;
}
declare class DotvvmEvents {
    init: DotvvmEvent<DotvvmEventArgs>;
    beforePostback: DotvvmEvent<DotvvmBeforePostBackEventArgs>;
    afterPostback: DotvvmEvent<DotvvmAfterPostBackEventArgs>;
    error: DotvvmEvent<DotvvmErrorEventArgs>;
    spaNavigating: DotvvmEvent<DotvvmSpaNavigatingEventArgs>;
    spaNavigated: DotvvmEvent<DotvvmSpaNavigatedEventArgs>;
    redirect: DotvvmEvent<DotvvmRedirectEventArgs>;
}
declare class DotvvmEvent<T extends DotvvmEventArgs> {
    name: string;
    private triggerMissedEventsOnSubscribe;
    private handlers;
    private history;
    constructor(name: string, triggerMissedEventsOnSubscribe?: boolean);
    subscribe(handler: (data: T) => void): void;
    unsubscribe(handler: (data: T) => void): void;
    trigger(data: T): void;
}
declare class DotvvmEventArgs {
    viewModel: any;
    constructor(viewModel: any);
}
declare class DotvvmErrorEventArgs extends DotvvmEventArgs {
    viewModel: any;
    xhr: XMLHttpRequest;
    isSpaNavigationError: boolean;
    handled: boolean;
    constructor(viewModel: any, xhr: XMLHttpRequest, isSpaNavigationError?: boolean);
}
declare class DotvvmBeforePostBackEventArgs extends DotvvmEventArgs {
    sender: HTMLElement;
    viewModel: any;
    viewModelName: string;
    validationTargetPath: any;
    postbackClientId: number;
    cancel: boolean;
    clientValidationFailed: boolean;
    constructor(sender: HTMLElement, viewModel: any, viewModelName: string, validationTargetPath: any, postbackClientId: number);
}
declare class DotvvmAfterPostBackEventArgs extends DotvvmEventArgs {
    sender: HTMLElement;
    viewModel: any;
    viewModelName: string;
    validationTargetPath: any;
    serverResponseObject: any;
    postbackClientId: number;
    commandResult: any;
    isHandled: boolean;
    wasInterrupted: boolean;
    constructor(sender: HTMLElement, viewModel: any, viewModelName: string, validationTargetPath: any, serverResponseObject: any, postbackClientId: number, commandResult?: any);
}
declare class DotvvmSpaNavigatingEventArgs extends DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    newUrl: string;
    cancel: boolean;
    constructor(viewModel: any, viewModelName: string, newUrl: string);
}
declare class DotvvmSpaNavigatedEventArgs extends DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    serverResponseObject: any;
    isHandled: boolean;
    constructor(viewModel: any, viewModelName: string, serverResponseObject: any);
}
declare class DotvvmRedirectEventArgs extends DotvvmEventArgs {
    viewModel: any;
    viewModelName: string;
    url: string;
    replace: boolean;
    isHandled: boolean;
    constructor(viewModel: any, viewModelName: string, url: string, replace: boolean);
}
declare class DotvvmFileUpload {
    showUploadDialog(sender: HTMLElement): void;
    private getIframe(sender);
    private openUploadDialog(iframe);
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
    format(format: string, ...values: string[]): string;
    formatString(format: string, value: any): string;
    parseDotvvmDate(value: string): Date | null;
    parseNumber(value: string): number;
    parseDate(value: string, format: string, previousValue?: Date): Date;
    bindingDateToString(value: KnockoutObservable<string | Date> | string | Date, format?: string): string | KnockoutComputed<string>;
    bindingNumberToString(value: KnockoutObservable<string | number> | string | number, format?: string): string | KnockoutComputed<string>;
}
declare class DotvvmPostBackHandler {
    execute(callback: () => void, sender: HTMLElement): void;
}
declare class ConfirmPostBackHandler extends DotvvmPostBackHandler {
    message: string;
    constructor(message: string);
    execute(callback: () => void, sender: HTMLElement): void;
}
declare class DotvvmPostBackHandlers {
    confirm: (options: any) => ConfirmPostBackHandler;
}
interface IDotvvmPostBackHandlerConfiguration {
    name: string;
    options: () => any;
}
declare enum DotvvmPromiseState {
    Pending = 0,
    Done = 1,
    Failed = 2,
}
interface IDotvvmPromise<TArg> {
    state: DotvvmPromiseState;
    done(callback: (arg: TArg) => void): any;
    fail(callback: (error: any) => void): any;
}
declare class DotvvmPromise<TArg> implements IDotvvmPromise<TArg> {
    private callbacks;
    private errorCallbacks;
    state: DotvvmPromiseState;
    private argument;
    private error;
    done(callback: (arg: TArg) => void, forceAsync?: boolean): void;
    fail(callback: (error) => void, forceAsync?: boolean): this;
    resolve(arg: TArg): this;
    reject(error: any): this;
    chainFrom(promise: IDotvvmPromise<TArg>): this;
}
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
    private findObject(obj, matcher);
    flatSerialize(viewModel: any): any;
    getPureObject(viewModel: any): {};
    private pad(value, digits);
    serializeDate(date: string | Date, convertToUtc?: boolean): string;
}
interface Document {
    getElementByDotvvmId(id: string): HTMLElement;
}
interface IRenderedResourceList {
    [name: string]: string;
}
interface IDotvvmPostbackScriptFunction {
    (pageArea: string, sender: HTMLElement, pathFragments: string[], controlId: string, useWindowSetTimeout: boolean, validationTarget: string, context: any, handlers: IDotvvmPostBackHandlerConfiguration[]): void;
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
declare class DotVVM {
    private postBackCounter;
    private fakeRedirectAnchor;
    private resourceSigns;
    private isViewModelUpdating;
    viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    };
    isSpaReady: KnockoutObservable<boolean>;
    viewModels: IDotvvmViewModels;
    culture: string;
    serialization: DotvvmSerialization;
    postBackHandlers: DotvvmPostBackHandlers;
    events: DotvvmEvents;
    globalize: DotvvmGlobalize;
    evaluator: DotvvmEvaluator;
    domUtils: DotvvmDomUtils;
    fileUpload: DotvvmFileUpload;
    validation: DotvvmValidation;
    extensions: IDotvvmExtensions;
    isPostbackRunning: KnockoutObservable<boolean>;
    init(viewModelName: string, culture: string): void;
    private handleHashChange(viewModelName, spaPlaceHolder, isInitialPageLoad);
    private postbackScript(bindingId);
    private persistViewModel(viewModelName);
    private backUpPostBackConter();
    private isPostBackStillActive(currentPostBackCounter);
    staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback?: (_: any) => void, errorCallback?: (xhr: XMLHttpRequest, error?: any) => void): void;
    private processPassedId(id, context);
    applyPostbackHandlers<T>(callback: () => IDotvvmPromise<T>, sender: HTMLElement, handlers?: IDotvvmPostBackHandlerConfiguration[], context?: any): IDotvvmPromise<T>;
    postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, useWindowSetTimeout: boolean, validationTargetPath?: any, context?: any, handlers?: IDotvvmPostBackHandlerConfiguration[], commandArgs?: any[]): IDotvvmPromise<DotvvmAfterPostBackEventArgs>;
    private error(viewModel, xhr, promise?);
    private loadResourceList(resources, callback);
    private loadResourceElements(elements, offset, callback);
    private getSpaPlaceHolder();
    private navigateCore(viewModelName, url);
    private handleRedirect(resultObject, viewModelName, replace?);
    private performRedirect(url, replace);
    private fixSpaUrlPrefix(url);
    private removeVirtualDirectoryFromUrl(url, viewModelName);
    private addLeadingSlash(url);
    private concatUrl(url1, url2);
    patch(source: any, patch: any): any;
    private updateDynamicPathFragments(context, path);
    private postJSON(url, method, postData, success, error, preprocessRequest?);
    private getJSON(url, method, spaPlaceHolderUniqueId, success, error);
    getXHR(): XMLHttpRequest;
    private cleanUpdatedControls(resultObject, updatedControls?);
    private restoreUpdatedControls(resultObject, updatedControls, applyBindingsOnEachControl);
    unwrapArrayExtension(array: any): any;
    buildRouteUrl(routePath: string, params: any): string;
    private isPostBackProhibited(element);
    private addKnockoutBindingHandlers();
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
    private addValidationError(validatedProperty, error);
}
declare var dotvvm: DotVVM;
declare class DotvvmEvaluator {
    evaluateOnViewModel(context: any, expression: any): any;
    evaluateOnContext(context: any, expression: string): any;
    getDataSourceItems(viewModel: any): any;
    tryEval(func: () => any): any;
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
