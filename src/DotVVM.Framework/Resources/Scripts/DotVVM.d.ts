/// <reference path="typings/virtual-dom/virtual-dom.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
interface KnockoutStatic {
    originalContextFor: (node: Element) => KnockoutBindingContext;
}
declare namespace DotvvmKnockoutCompat {
    const nonControllingBindingHandlers: {
        [bindingName: string]: boolean;
    };
    function createKnockoutContext(dataContext: KnockoutObservable<RenderContext<any>>): KnockoutBindingContext;
    function wrapInObservables(objOrObservable: any, update?: ((updater: StateUpdate<any>) => void) | null): any;
    class KnockoutBindingHook implements virtualDom.VHook {
        readonly dataContext: RenderContext<any>;
        constructor(dataContext: RenderContext<any>);
        private lastState;
        hook(node: Element, propertyName: string, previousValue: this): void;
        unhoook(node: Element, propertyName: string, nextValue: any): any;
    }
    class KnockoutBindingWidget implements virtualDom.Widget {
        readonly dataContext: RenderContext<any>;
        readonly node: virtualDom.VNode;
        readonly nodeChildren: ((context: RenderContext<any>) => virtualDom.VTree)[] | null;
        readonly dataBind: string | null;
        readonly koComments: {
            start: number;
            end: number;
            dataBind: string;
        }[];
        readonly type: string;
        elementId: string;
        contentMapping: {
            element: Node;
            index: number;
            lastDom: virtualDom.VTree;
        }[] | null;
        lastState: KnockoutObservable<RenderContext<any>>;
        domWatcher: MutationObserver;
        constructor(dataContext: RenderContext<any>, node: virtualDom.VNode, nodeChildren: ((context: RenderContext<any>) => virtualDom.VTree)[] | null, dataBind: string | null, koComments: {
            start: number;
            end: number;
            dataBind: string;
        }[]);
        getFakeContent(): Node[];
        init(): Element;
        private setupDomWatcher(element);
        private static knockoutInternalDataPropertyName;
        private copyKnockoutInternalDataProperty(from, to);
        private isElementRooted(element, root);
        private replaceTmpSpans(nodes, rootElement);
        private removeRemovedNodes(rootElement);
        update(previousWidget: this, previousDomNode: Element): Element | undefined;
        destroy(domNode: Element): void;
        static getBetterContext(dataContext: KnockoutBindingContext): RenderContext<any>;
    }
    function createDecorator(element: Element): ((element: RendererInitializer.RenderNodeAst) => RendererInitializer.AssignedPropDescriptor) | undefined;
}
declare type StateUpdate<TViewModel> = (initial: TViewModel) => TViewModel;
declare type RenderContext<TViewModel> = {
    update: (updater: StateUpdate<TViewModel>) => void;
    dataContext: TViewModel;
    parentContext?: RenderContext<any>;
    "@extensions"?: {
        [name: string]: any;
    };
};
declare type RenderFunction<TViewModel> = (context: RenderContext<TViewModel>) => virtualDom.VTree;
declare class TwoWayBinding<T> {
    readonly update: (updater: StateUpdate<T>) => void;
    readonly value: T;
    constructor(update: (updater: StateUpdate<T>) => void, value: T);
}
declare const createArray: <T>(a: {
    [i: number]: T;
}) => T[];
declare class HtmlElementPatcher {
    element: HTMLElement;
    private previousDom;
    constructor(element: HTMLElement, initialDom: virtualDom.VNode | null);
    applyDom(dom: virtualDom.VNode): void;
}
declare class Renderer<TViewModel> {
    readonly renderFunctions: RenderFunction<TViewModel>[];
    readonly vdomDispatcher: (dom: virtualDom.VNode[]) => void;
    readonly renderedStateObservable: any;
    private _state;
    readonly state: TViewModel;
    private _isDirty;
    readonly isDirty: boolean;
    constructor(initialState: TViewModel, renderFunctions: RenderFunction<TViewModel>[], vdomDispatcher: (dom: virtualDom.VNode[]) => void);
    dispatchUpdate(): void;
    private startTime;
    private rerender(time);
    setState(newState: TViewModel): TViewModel;
    update(updater: StateUpdate<TViewModel>): TViewModel;
}
declare namespace RendererInitializer {
    type ConstantOrFunction<T> = {
        readonly type: "constant";
        readonly constant: T;
    } | {
        readonly type: "func";
        readonly dataContextDepth: number;
        readonly elements: RenderNodeAst[];
        readonly func: (dataContext: RenderContext<any>, elements: virtualDom.VTree[]) => T;
    };
    interface AttrDescriptor {
        name: ConstantOrFunction<string>;
        value: ConstantOrFunction<any>;
    }
    type AssignedPropDescriptor = {
        readonly type: "attr";
        readonly attr: AttrDescriptor;
    } | {
        readonly type: "decorator";
        readonly fn: ConstantOrFunction<(node: virtualDom.VTree) => virtualDom.VTree>;
    };
    type RenderNodeAst = ConstantOrFunction<virtualDom.VTree> | {
        readonly type: "ast";
        readonly name: ConstantOrFunction<string>;
        readonly attributes: AttrDescriptor[];
        readonly content: RenderNodeAst[];
    } | {
        readonly type: "text";
        readonly content: ConstantOrFunction<string>;
    };
    const astConstant: <T>(val: T) => ConstantOrFunction<T>;
    const astFunc: <T>(dataContextDepth: number, elements: RenderNodeAst[], func: (dataContext: RenderContext<any>, elements: virtualDom.VTree[]) => T) => ConstantOrFunction<T>;
    const mapConstantOrFunction: <T, U>(source: ConstantOrFunction<T>, map: (val: T, myElements: virtualDom.VTree[]) => U, myElements: RenderNodeAst[]) => ConstantOrFunction<U>;
    const createRenderFunction: <TViewModel>(ast: RenderNodeAst) => RenderFunction<TViewModel>;
    function initFromNode<TViewModel>(elements: Element[], viewModel: TViewModel): Renderer<TViewModel>;
}
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
    sender: HTMLElement | undefined;
    viewModel: any;
    viewModelName: string;
    validationTargetPath: any;
    serverResponseObject: any;
    postbackClientId: number;
    commandResult: any;
    isHandled: boolean;
    wasInterrupted: boolean;
    constructor(sender: HTMLElement | undefined, viewModel: any, viewModelName: string, validationTargetPath: any, serverResponseObject: any, postbackClientId: number, commandResult?: any);
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
declare type DotvvmPostbackHandler2 = {
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T>;
};
declare type PostbackRejectionReason = {
    type: "handler";
    handler: DotvvmPostbackHandler2 | DotvvmPostBackHandler;
    message?: string;
} | {
    type: 'network';
    error: DotvvmErrorEventArgs;
} | {
    type: 'commit';
    error: DotvvmErrorEventArgs;
} | {
    type: 'event';
};
declare class DotvvmPostBackHandler {
    execute<T>(callback: () => void, sender: HTMLElement): void;
}
declare class ConfirmPostBackHandler extends DotvvmPostBackHandler {
    message: string;
    constructor(message: string);
    execute(callback: () => void, sender: HTMLElement): void;
}
declare class PostbackOptions {
    readonly postbackId: number;
    readonly sender: HTMLElement;
    readonly args: any[];
    readonly viewModel: any;
    readonly viewModelName: string;
    readonly validationTargetPath: any;
    constructor(postbackId: number, sender?: HTMLElement, args?: any[], viewModel?: any, viewModelName?: string, validationTargetPath?: any);
}
declare class ConfirmPostBackHandler2 implements DotvvmPostbackHandler2 {
    message: string;
    constructor(message: string);
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T>;
}
interface IDotvvmPostBackHandlerConfiguration {
    name: string;
    options: () => any;
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
declare type IDotvvmStateRoot<T> = {
    readonly viewModel: T;
    readonly renderedResources: string[];
    readonly url: string;
    readonly virtualDirectory: string;
};
declare class DotVVM {
    private postBackCounter;
    private lastStartedPostack;
    private fakeRedirectAnchor;
    private resourceSigns;
    private isViewModelUpdating;
    receivedViewModel: IDotvvmViewModelInfo;
    isSpaReady: KnockoutObservable<boolean>;
    private _viewModels;
    readonly viewModels: IDotvvmViewModels;
    private _viewModelObservables;
    readonly viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    };
    rootRenderer: Renderer<any>;
    culture: string;
    serialization: DotvvmSerialization;
    postBackHandlers: {
        [name: string]: ((options: any) => DotvvmPostBackHandler);
    };
    postbackHandlers2: {
        [name: string]: ((options: any) => DotvvmPostbackHandler2);
    };
    private beforePostbackEventPostbackHandler;
    private afterPostbackEventpostbackHandler;
    private isPostBackRunningHandler;
    private windowsSetTimeoutHandler;
    private defaultConcurrencyPostbackHandler;
    globalPostbackHandlers: (IDotvvmPostBackHandlerConfiguration | string | DotvvmPostbackHandler2)[];
    globalLaterPostbackHandlers: (IDotvvmPostBackHandlerConfiguration | string | DotvvmPostbackHandler2)[];
    private convertOldHandler(handler);
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
    protected getPostbackHandler(name: string): (options: any) => DotvvmPostbackHandler2;
    private isPostbackHandler(obj);
    findPostbackHandlers(knockoutContext: any, config: (IDotvvmPostBackHandlerConfiguration | string | DotvvmPostbackHandler2)[]): DotvvmPostbackHandler2[];
    applyPostbackHandlers<T>(callback: (options: PostbackOptions) => Promise<T>, sender: HTMLElement, handlers?: (IDotvvmPostBackHandlerConfiguration | string | DotvvmPostbackHandler2)[], args?: any[], validationPath?: any, context?: any, viewModel?: any, viewModelName?: string): Promise<T>;
    postbackCore(viewModelName: string, options: PostbackOptions, path: string[], command: string, controlUniqueId: string, context: any, validationTargetPath?: any, commandArgs?: any[]): Promise<() => Promise<DotvvmAfterPostBackEventArgs>>;
    postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, useWindowSetTimeout: boolean, validationTargetPath?: any, context?: any, handlers?: (IDotvvmPostBackHandlerConfiguration | string | DotvvmPostbackHandler2)[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs>;
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
    buildUrlSuffix(urlSuffix: string, query: any): string;
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
