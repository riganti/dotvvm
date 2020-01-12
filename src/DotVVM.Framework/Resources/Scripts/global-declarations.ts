// not a module, available to everyone

type PostbackCommitFunction = () => Promise<DotvvmAfterPostBackEventArgs>

type DotvvmPostbackHandler = {
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions): Promise<PostbackCommitFunction>
    name?: string
    after?: Array<string | DotvvmPostbackHandler>
    before?: Array<string | DotvvmPostbackHandler>
}
type PostbackRejectionReason =
    | { type: "handler", handlerName: string, message?: string }
    | { type: 'network', err?: any }
    | { type: 'commit', args: DotvvmErrorEventArgs }
    | { type: 'csrfToken' }
    | { type: 'invalidJson', responseText: string }
    | { type: 'serverError', status?: number, responseObject: any }
    | { type: 'event' }
    & { options?: PostbackOptions }

interface AdditionalPostbackData {
    [key: string]: any
    validationTargetPath?: string
}

type PostbackOptions = {
    readonly additionalPostbackData: AdditionalPostbackData
    readonly postbackId: number
    readonly sender?: HTMLElement
    readonly args: any[]
    readonly viewModel?: any
}

type PostbackEventArgs = DotvvmEventArgs & {
    readonly postbackClientId: number
    readonly sender?: Element
    readonly xhr?: XMLHttpRequest
    readonly serverResponseObject?: any
    readonly postbackOptions: PostbackOptions
}

type DotvvmEventArgs = {
    /** The global view model */
    readonly viewModel?: any
}

type DotvvmErrorEventArgs = {
    readonly sender?: Element
    readonly serverResponseObject?: any
    readonly viewModel?: any
    readonly isSpaNavigationError?: true
    handled: boolean
}

type DotvvmBeforePostBackEventArgs = PostbackEventArgs & {
    cancel: boolean
}
type DotvvmAfterPostBackEventArgs = PostbackEventArgs & {
    handled: boolean
    /** Set to true in case the postback did not finish and it was cancelled by an event or a postback handler */
    readonly wasInterrupted: boolean;
    readonly serverResponseObject: any
    readonly commandResult: any
    /** In SPA mode, this promise is set when the result of a postback is a redirection. */
    readonly redirectPromise?: Promise<DotvvmNavigationEventArgs>
}
type DotvvmSpaNavigatingEventArgs = DotvvmEventArgs & {
    /** When set to true by an event handler, it  */
    cancel: boolean
    /** The url we are navigating to */
    readonly newUrl: string
}
type DotvvmNavigationEventArgs = DotvvmEventArgs & {
    readonly serverResponseObject: any
    readonly xhr?: XMLHttpRequest // TODO:
    readonly isSpa?: true
}
type DotvvmSpaNavigatedEventArgs = DotvvmNavigationEventArgs & {
    /** When error occurs, this is set to false and gives the event handlers a possibility to mark the error as handled */
    isHandled: boolean
}
type DotvvmRedirectEventArgs = DotvvmEventArgs & {
    /** The url of the page we are navigating to */
    readonly url: string
    /** Whether the new url should replace the current url in the browsing history */
    readonly replace: boolean
}

interface DotvvmViewModelInfo {
    viewModel?: any
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
