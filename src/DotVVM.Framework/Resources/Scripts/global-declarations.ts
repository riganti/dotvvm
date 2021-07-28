// not a module, available to everyone

type PostbackCommitFunction = (...args: any) => Promise<DotvvmAfterPostBackEventArgs>

type DotvvmPostbackHandler = {
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions): Promise<PostbackCommitFunction>
    name?: string
    after?: Array<string | DotvvmPostbackHandler>
    before?: Array<string | DotvvmPostbackHandler>
}

type DotvvmPostbackErrorLike = {
    readonly reason: DotvvmPostbackErrorReason
}

type DotvvmPostbackErrorReason =
    | { type: 'handler', handlerName: string, message?: string }
    | { type: 'network', err?: any }
    | { type: 'gate' }
    | { type: 'commit', args?: DotvvmErrorEventArgs }
    | { type: 'csrfToken' }
    | { type: 'serverError', status?: number, responseObject: any, response?: Response }
    | { type: 'event' }
    | { type: 'validation', responseObject: any, response?: Response }
    | { type: 'abort' }
    & { options?: PostbackOptions }

type PostbackCommandType = "postback" | "staticCommand" | "spaNavigation"

type PostbackOptions = {
    readonly postbackId: number
    readonly commandType: PostbackCommandType
    readonly args: any[]
    readonly sender?: HTMLElement
    readonly viewModel?: any
    serverResponseObject?: any
    validationTargetPath?: string,
    abortSignal?: AbortSignal
}

type DotvvmErrorEventArgs = PostbackOptions & {
    readonly response?: Response
    readonly error: DotvvmPostbackErrorLike
    handled: boolean
}

type DotvvmBeforePostBackEventArgs = PostbackOptions & {
    cancel: boolean
}

type DotvvmAfterPostBackEventArgs = PostbackOptions & {
    /** Set to true in case the postback did not finish and it was cancelled by an event or a postback handler */
    readonly wasInterrupted?: boolean;
    readonly commandResult?: any
    readonly response?: Response
    readonly error?: DotvvmPostbackErrorLike
}

type DotvvmNavigationEventArgs = PostbackOptions & {
    readonly url: string
}

type DotvvmSpaNavigatingEventArgs = DotvvmNavigationEventArgs & {
    cancel: boolean
}

type DotvvmSpaNavigatedEventArgs = DotvvmNavigationEventArgs & {
    readonly response?: Response
}

type DotvvmSpaNavigationFailedEventArgs = DotvvmNavigationEventArgs & {
    readonly response?: Response
    readonly error?: DotvvmPostbackErrorLike
}

type DotvvmRedirectEventArgs = DotvvmNavigationEventArgs & {
    readonly response?: Response
    /** Whether the new url should replace the current url in the browsing history */
    readonly replace: boolean
}

type DotvvmPostbackHandlersStartedEventArgs = PostbackOptions & {
}

type DotvvmPostbackHandlersCompletedEventArgs = PostbackOptions & {
}

type DotvvmPostbackResponseReceivedEventArgs = PostbackOptions & {
    readonly response: Response
}

type DotvvmPostbackCommitInvokedEventArgs = PostbackOptions & {
    readonly response: Response
}

type DotvvmPostbackViewModelUpdatedEventArgs = PostbackOptions & {
    readonly response: Response
}

type DotvvmPostbackRejectedEventArgs = PostbackOptions & {
    readonly error: DotvvmPostbackErrorLike
}

type DotvvmStaticCommandMethodEventArgs = PostbackOptions & {
    readonly methodId: string
    readonly methodArgs: any[]
}

type DotvvmStaticCommandMethodInvokingEventArgs = DotvvmStaticCommandMethodEventArgs & {
}

type DotvvmStaticCommandMethodInvokedEventArgs = DotvvmStaticCommandMethodEventArgs & {
    readonly result: any
    readonly response?: Response
}

type DotvvmStaticCommandMethodFailedEventArgs = DotvvmStaticCommandMethodEventArgs & {
    readonly result?: any
    readonly response?: Response
    readonly error: DotvvmPostbackErrorLike
}

type DotvvmInitEventArgs = {
    readonly viewModel: any
}

type DotvvmInitCompletedEventArgs = {
}

interface DotvvmViewModelInfo {
    viewModel?: any
    viewModelCacheId?: string
    viewModelCache?: any
    renderedResources?: string[]
    url?: string
    virtualDirectory?: string
    typeMetadata: TypeMap
}

interface DotvvmViewModels {
    [name: string]: DotvvmViewModelInfo
    root: DotvvmViewModelInfo
}

interface DotvvmPostbackHandlerCollection {
    [name: string]: ((options: any) => DotvvmPostbackHandler);
}

type DotvvmStaticCommandResponse<T = any> = {
    result: any;
    customData: { [key: string]: any };
    typeMetadata?: TypeMap;
} | {
    action: "redirect",
    url: string,
    replace?: boolean,
    allowSpa?: boolean
};

type DotvvmPostBackHandlerConfiguration = {
    name: string;
    options: (context: KnockoutBindingContext) => any;
}

type ClientFriendlyPostbackHandlerConfiguration =
    | string // just a name
    | DotvvmPostbackHandler // the handler itself
    | DotvvmPostBackHandlerConfiguration // the verbose configuration
    | [string, object] // compressed configuration - [name, handler options]
    | [string, (context: KnockoutBindingContext, data: any) => any] // compressed configuration with binding support - [name, context => handler options]

type PropertyValidationRuleInfo = {
    ruleName: string;
    errorMessage: string;
    parameters: any[];
}

type ValidationRuleTable = {
    [type: string]: {
        [property: string]: [PropertyValidationRuleInfo]
    }
}

type StateUpdate<TViewModel> = (initial: DeepReadonly<TViewModel>) => DeepReadonly<TViewModel>
type UpdateDispatcher<TViewModel> = (update: StateUpdate<TViewModel>) => void

/** Knockout observable, including all child object and arrays */
type DeepKnockoutObservable<T> =
    T extends (infer R)[] ? DeepKnockoutObservableArray<R> :
    T extends object      ? KnockoutObservable<DeepKnockoutObservableObject<T>> :
                            KnockoutObservable<T>;
type DeepKnockoutObservableArray<T> = KnockoutObservableArray<DeepKnockoutObservable<T>>
type DeepKnockoutObservableObject<T> = {
    readonly [P in keyof T]: DeepKnockoutObservable<T[P]>;
}

/** Partial<T>, but including all child objects  */
type DeepPartial<T> =
    T extends object ? { [P in keyof T]?: DeepPartial<T[P]>; } :
    T;
/** Readonly<T>, but including all child objects and arrays  */
type DeepReadonly<T> =
    T extends TypeDefinition ? T :
    T extends (infer R)[] ? readonly DeepReadonly<R>[] :
    T extends object ? { readonly [P in keyof T]: DeepReadonly<T[P]>; } :
    T;

/** Knockout observable that is found in the DotVVM ViewModel - all nested objects and arrays are also observable + it has some helper functions (state, patchState, ...) */
type DotvvmObservable<T> = DeepKnockoutObservable<T> & {
    /** A property, returns latest state from dotvvm.state. It does not contain any knockout observable and does not have any propagation delay, as the value in the observable */
    readonly state: DeepReadonly<T>
    /** Sets new state directly into the dotvvm.state.
     * Note that the value arrives into the observable itself asynchronously, so there might be slight delay */
    readonly setState: (newState: DeepReadonly<T>) => void
    /** Patches the current state and sets it into dotvvm.state.
     * Compared to setState, when property does not exist in the patch parameter, the old value from state is used.
     * Note that the value arrives into the observable itself asynchronously, so there might be slight delay
     * @example observable.patchState({ Prop2: 0 }) // Only must be specified, although Prop1 also exists and is required  */
    readonly patchState: (patch: DeepReadonly<DeepPartial<T>>) => void
    /** Dispatches update of the state.
     * Note that the value arrives into the observable itself asynchronously, so there might be slight delay
     * @example observable.updater(state => [ ...state, newElement ]) // This appends an element to an (observable) array
     * @example observable.updater(state => state + 1) // Increments the value by one
     * @example observable.updater(state => ({ ...state, MyProperty: state.MyProperty + 1 })) // Increments the property MyProperty by one
     */
    readonly updater: UpdateDispatcher<T>
}

type RootViewModel = {
    $type: string
    $csrfToken?: string
    [name: string]: any
} 

type TypeMap = {
    [typeId: string]: TypeMetadata
}

type ObjectTypeMetadata = {
    type: "object",
    properties: { [prop: string]: PropertyMetadata }
}

type EnumTypeMetadata = {
    type: "enum",
    values: { [name: string]: number },
    isFlags?: boolean
}

type TypeMetadata = ObjectTypeMetadata | EnumTypeMetadata;

type PropertyMetadata = {
    type: TypeDefinition;
    post?: "always" | "pathOnly" | "no";
    update?: "always" | "firstRequest" | "no";
    validationRules?: PropertyValidationRuleInfo[];
    clientExtenders?: ClientExtenderInfo[]
}

type TypeDefinition = string |
{ readonly type: "nullable", readonly inner: TypeDefinition } |
{ readonly type: "dynamic" } |
    TypeDefinition[];

type ClientExtenderInfo = {
    name: string,
    parameter: any
}

type CoerceErrorType = {
    isError: true
    wasCoerced: false
    message: string
    path: string
    prependPathFragment(fragment: string): void
    value: never
}

type CoerceResult = CoerceErrorType | { value: any, wasCoerced?: boolean, isError?: false };

type DotvvmFileUploadCollection = {
    Files: KnockoutObservableArray<KnockoutObservable<DotvvmFileUploadData>>;
    Progress: KnockoutObservable<number>;
    Error: KnockoutObservable<string>;
    IsBusy: KnockoutObservable<boolean>;
}
type DotvvmFileUploadData = {
    FileId: KnockoutObservable<string>;
    FileName: KnockoutObservable<string>;
    FileSize: KnockoutObservable<DotvvmFileSize>;
    IsFileTypeAllowed: KnockoutObservable<boolean>;
    IsMaxSizeExceeded: KnockoutObservable<boolean>;
    IsAllowed: KnockoutObservable<boolean>;
}
type DotvvmFileSize = {
    Bytes: KnockoutObservable<number>;
    FormattedText: KnockoutObservable<string>;
}
