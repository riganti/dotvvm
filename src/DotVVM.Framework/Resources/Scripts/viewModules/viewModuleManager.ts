import { keys } from "../utils/objects";

type ModuleCommand = (context: ModuleContext, ...args: any[]) => Promise<any>;
type ModuleCommandDictionary = { [name: string]: ModuleCommand };

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
        handler.contexts[viewId].elements.push(rootElement);
        setupModuleDisposeHandlers(viewId, name, rootElement);
        return;
    }

    const elementContext = ko.contextFor(rootElement);

    console.info(rootElement);

    let exportedCommands: ModuleCommandDictionary = {};

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
        [rootElement],
        { ...elementContext.$control }
    );
    handler.contexts[viewId] = context;

    callIfDefined(handler.module, 'init', context);
}

export function callViewModuleCommand(viewId: string, commandName: string, args: any[]) {
    if (commandName == null) { throw new Error("commandName has to have a value"); }

    const moduleNames: string[] = [];

    const foundCommands: {
        command: ModuleCommand,
        context: ModuleContext
    }[] = [];

    for (const moduleName of keys(registeredModules)) {
        const context = ensureViewModuleContext(viewId, moduleName);
        const command: ModuleCommand = context.moduleCommands[commandName];

        moduleNames.push(moduleName);
        foundCommands.push({ command, context });
    }

    if (foundCommands.length < 1) {
        throw new Error('Command ' + commandName + 'could not be found in any of the imported modules in view ' + viewId + '.');
    }

    if (foundCommands.length > 1) {
        throw new Error(
            'Conflict: There were multiple commands named '
            + commandName +
            ' the in imported modules in view '
            + viewId +
            '. Check modules: ' + moduleNames.join(', ') + '.');
    }

    foundCommands[0].command.apply(window, [foundCommands[0].context as any].concat(args) as any);
}

export function registerNamedCommand(viewId: string, commandName: string, command: ModuleCommand) {
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
        disposeModule(viewId, name, rootElement);
        ko.utils.domNodeDisposal.removeDisposeCallback(rootElement, elementDisposeCallback);
    }
    ko.utils.domNodeDisposal.addDisposeCallback(rootElement, elementDisposeCallback);
}

function disposeModule(viewId: string, name: string, rootElement: HTMLElement) {
    const handler = ensureModuleHandler(name);
    const context = ensureViewModuleContext(viewId, name);

    const index = context.elements.indexOf(rootElement);
    if (index < 0) {
        throw new Error(`Cannot dispose module on a root element ${viewId}. It has already been disposed.`);
    }
    context.elements.splice(index, 1);

    if (!context.elements.length) {
        callIfDefined(handler.module, 'dispose', context);
        delete handler.contexts[viewId];
    }
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
        public readonly moduleCommands: ModuleCommandDictionary,
        public readonly viewId: string,
        public readonly elements: HTMLElement[],
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
