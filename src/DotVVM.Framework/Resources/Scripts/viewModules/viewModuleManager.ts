import register from "../binding-handlers/register";
import { initCore, getViewModel, getViewModelObservable, initBindings, getCulture } from "../dotvvm-base"
import { keys } from "../utils/objects";

let registeredModules: { [name: string]: ModuleHandler } = {};

export function registerViewModule(name: string, moduleObject: any) {
    if (name == null) { throw new Error("Parameter name has to have a value"); }
    if (moduleObject == null) { throw new Error("Parameter moduleObject has to have a value"); }

    //If multiple views on the same page use the module, we only want to register it the first time,
    //the other views then get initialized on their own
    if (!registeredModules[name]) {
        registeredModules[name] = new ModuleHandler(name, moduleObject);
    }
}

export function registerViewModules(modules: { [name: string]: any }) {
    for (const moduleName of keys(modules)) {
        const moduleObject = modules[moduleName];
        registerViewModule(moduleName, moduleObject);
    }
}

export function initViewModule(name: string, viewId: string, rootElement: HTMLElement) {

    if (viewId == null) { throw new Error("viewId has to have a value"); }
    if (rootElement == null) { throw new Error("rootElement has to have a value"); }

    const handler = ensureModuleHandler(name);

    console.info(handler);

    if (handler.contexts[viewId]) {
        throw new Error('Handler ' + name + ' has already been initialized.');
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

    setupModuleDisposeHandlers(viewId, name, rootElement);

    var context = new ModuleContext(
        name,
        handler.module,
        exportedCommands,
        viewId,
        rootElement,
        elementContext.$data,
        { ...elementContext.$control }
    );
    handler.contexts[viewId] = context;

    callIfDefined(handler.module, 'init', context);
}

export function callViewModuleCommand(viewId: string, moduleName: string, commandName: string, ...args: any[]) {
    if (commandName == null) { throw new Error("commandName has to have a value"); }

    const context = ensureViewModuleContext(viewId, moduleName);
    const command = context.moduleCommands[commandName];

    if (!command) {
        throw new Error('Command ' + commandName + 'could not be found in module ' + moduleName + ' view ' + viewId + '.');
    }

    command(context, args);
}

export function registerNamedCommand(viewId: string, commandName: string, command: (context: ModuleContext, ...args: any[]) => Promise<any>) {
    if (viewId == null) { throw new Error("Parameter viewId has to have a value"); }
    if (commandName == null) { throw new Error("Parameter commandName has to have a value"); }

    for (const moduleName of keys(registeredModules)) {
        const module = registeredModules[moduleName];

        var context = module.contexts[viewId];
        if (context) {
            context.registerNamedCommand(commandName, command);
        }
    }
}

function setupModuleDisposeHandlers(viewId: string, name: string, rootElement: HTMLElement) {
    function elementDisposeCallback() {
        disposeModule(viewId, name);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

function disposeModule(viewId: string, name: string) {
    const handler = ensureModuleHandler(name);
    const context = ensureViewModuleContext(viewId, name);

    callIfDefined(handler.module, 'dispose', context);
    delete handler.contexts[viewId];
}

function ensureViewModuleContext(viewId: string, name: string): ModuleContext {
    const handler = ensureModuleHandler(name);
    const context = handler.contexts[viewId];

    if (!context) {
        throw new Error('Module ' + name + 'has not been initialized for view ' + viewId + ', or the view has been disposed');
    }
    return context;
}

function ensureModuleHandler(name: string): ModuleHandler {
    if (name == null) { throw new Error("name has to have a value"); }

    const handler = registeredModules[name];

    if (!handler) {
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
    public contexts: { [viewId: string]: ModuleContext } = {};
    constructor(public readonly name: string, public readonly module: any) {
    }
}

export class ModuleContext {
    private readonly namedCommands: { [name: string]: (...args: any[]) => Promise<any> } = {};
    public readonly state: any = {};

    constructor(
        public readonly moduleName: string,
        public readonly module: any,
        public readonly moduleCommands: { [name: string]: (context: ModuleContext, ...args: any[]) => Promise<any> },
        public readonly viewId: string,
        public readonly element: HTMLElement,
        public readonly viewModel: any,
        public readonly properties: { [name: string]: any }) {
    }

    public callNamedCommand = (commandName: string, ...args: any[]) => {
        if (!commandName) { throw new Error("Parameter commandName has to have a value"); }

        var command = this.namedCommands[commandName];

        if (!command) {
            throw new Error('Could not find named command ' + commandName + ' registered for view ' + this.viewId + '. Make sure your named command registration is on the same view as the @js directive that imports module ' + this.moduleName + '.');
        }

        command(...args);
    };

    public registerNamedCommand = (name: string, command: (...args: any[]) => Promise<any>) => {
        if (name == null) {
            throw new Error("Parameter name has to have a value");
        }
        if (!command || typeof (command) !== 'function') {
            throw new Error('Named command has to be a function');
        }
        if (this.namedCommands[name]) {
            throw new Error('A named command is already registered under the name: ' + name + '. The conflict occured in: ' + this.moduleName + ' view ' + this.viewId + '.');
        }

        this.namedCommands[name] = command;
    }
}
