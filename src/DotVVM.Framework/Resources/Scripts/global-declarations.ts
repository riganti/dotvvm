// not a module, available to everyone

type PostbackCommitFunction = () => Promise<DotvvmAfterPostBackEventArgs>

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
    & { options?: PostbackOptions }

type PostbackCommandType = "postback" | "staticCommand" | "spaNavigation"

type PostbackOptions = {
    readonly postbackId: number
    readonly commandType: PostbackCommandType
    readonly args: any[]
    readonly sender?: HTMLElement
    readonly viewModel?: any
    serverResponseObject?: any
    validationTargetPath?: string
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
    readonly wasInterrupted: boolean;
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
    /** Whether the new url should replace the current url in the browsing history */
    readonly response?: Response
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
    readonly response?: Response
    readonly error: DotvvmPostbackErrorLike
}

type DotvvmStaticCommandMethodEventArgs = PostbackOptions & {
    readonly command: string
    readonly args: any[]
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

interface DotvvmViewModelInfo {
    viewModel?: any
    viewModelCacheId?: string
    viewModelCache?: any
    renderedResources?: string[]
    url?: string
    virtualDirectory?: string
}

interface DotvvmViewModels {
    [name: string]: DotvvmViewModelInfo
    root: DotvvmViewModelInfo
}

interface DotvvmPostbackHandlerCollection {
    [name: string]: ((options: any) => DotvvmPostbackHandler);
}

type DotvvmStaticCommandResponse = {
    result: any;
} | {
    action: "redirect";
    url: string;
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

type RootViewModel = {
    $csrfToken?: string | KnockoutObservable<string>,
}
