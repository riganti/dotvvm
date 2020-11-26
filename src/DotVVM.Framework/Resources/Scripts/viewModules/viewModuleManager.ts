import { initCore, getViewModel, getViewModelObservable, initBindings, getCulture } from "../dotvvm-base"
import { keys } from "../utils/objects";

let registeredModules: { [name: string]: ModuleHandler } = {};

export function registerViewModule(name: string, moduleObject: any) {
    if (name == null) { throw new Error("Parameter name has to have a value"); }
    if (moduleObject == null) { throw new Error("Parameter moduleObject has to have a value"); }

    registeredModules[name] = new ModuleHandler(name, moduleObject);
}

export function initViewModule(name: string, viewId: string, rootElement: HTMLElement) {

    if (viewId == null) { throw new Error("viewId has to have a value"); }
    if (rootElement == null) { throw new Error("rootElement has to have a value"); }

    const handler = ensureModuleHandler(name);

    console.info(handler);

    if (handler.isInitialized) {
        throw new Error('Handler '+name+ ' has already been initialized.');
    }

    const elementContext = ko.contextFor(rootElement);

    console.info(rootElement);

    let exportedCommands: { [name: string]: (context: ModuleContext, ...args: any[]) => any; } = {};

    if (handler.module.commands && typeof (handler.module.commands) === 'object') {
        var commandNames = keys(handler.module.commands);
        for (const commandName of commandNames) {
            var command = handler.module.commands[commandName];

            if (typeof (command) !== 'function') {
                console.error('Object ' + commandName + ' is not a function. Commands object is intended to only export functions. Object ' + commandName + 'skipped.');
                continue;
            }

            exportedCommands[commandName] = command;
        }
    }

    setupModuleDisposeHandlers(name, rootElement);

    handler.context = new ModuleContext(
        exportedCommands,
        {},
        {},
        viewId,
        rootElement,
        elementContext.$data,
        { ...elementContext.$control }
    );

    callIfDefined(handler.module, 'init', handler.context);
    handler.isInitialized = true;
}

export function callViewModuleCommand(moduleName: string, commandName: string, ...args: any[]) {
    if (commandName == null) { throw new Error("commandName has to have a value"); }

    const handler = ensureInitializedModuleHandler(moduleName);
    const command = handler.context?.moduleCommands[commandName];

    if (!command) {
        throw new Error('Command ' + commandName + 'could not be found in module ' + moduleName + '.');
    }

    command(handler.context as ModuleContext, args);
}

function setupModuleDisposeHandlers(name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        disposeModule(name);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

function disposeModule(name: string) {
    const handler = ensureInitializedModuleHandler(name);

    callIfDefined(handler.module, 'dispose', handler.context);
    delete registeredModules[name];
    handler.isDisposed = true;
}

function ensureInitializedModuleHandler(name: string): ModuleHandler {
    const handler = ensureModuleHandler(name);
    if (!handler.isInitialized) {
        throw new Error('Module ' + name + 'has not been initialized.');
    }
    return handler;
}

function ensureModuleHandler(name: string): ModuleHandler {
    if (name == null) { throw new Error("name has to have a value"); }

    const handler = registeredModules[name];

    console.info(!handler || handler.isDisposed);

    if (!handler || handler.isDisposed) {
        throw new Error('Could not find module ' + name + '. Module is not registered, or has been disposed.');
    }
    return handler;
}

function callIfDefined(module: any, name: string, ...args: any[]) {
    if (module[name] && typeof (module[name]) == 'function') {
        module[name](...args);
    }
}

class ModuleHandler {
    public isInitialized: boolean;
    public isDisposed: boolean;
    public context: ModuleContext | null;
    constructor(public readonly name: string, public readonly module: any) {
        this.isInitialized = false;
        this.isDisposed = false;
        this.context = null;
    }
}

export class ModuleContext {
    constructor(
        public readonly moduleCommands: { [name: string]: (context: ModuleContext, ...args: any[]) => Promise<any> },
        public readonly namedCommands: { [name: string]: (...args: any[]) => Promise<any> },
        public readonly state: any,
        public readonly viewId: string,
        public readonly element: HTMLElement,
        public readonly viewModel: any,
        public readonly properties: { [name: string]: any }) {

    }
}
