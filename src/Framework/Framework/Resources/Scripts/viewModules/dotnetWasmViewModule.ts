// DO NOT IMPORT THIS MODULE - it is not a part of DotVVM bundle, it is distributed separately at /dotvvmStaticResource/dotnetWasmInterop.js

// @ts-ignore 
import { dotnet } from "./dotnet.js";
declare var dotvvm: any;

let interop: any;
async function initDotnet() {
    const { setModuleImports, getAssemblyExports, getConfig } = await dotnet.withDiagnosticTracing(true).create();

    setModuleImports("dotvvmResource/dotvvm--interop--dotnet-wasm/dotvvm--interop--dotnet-wasm", {
        callNamedCommand: async (typeName: string, instanceName: string, commandName: string, args: string[]) => {
            const viewIdOrElement = instanceMap[instanceName];
            const argValues = args.map(a => JSON.parse(a));
            const result = await dotvvm.viewModules.call(viewIdOrElement, "callNamedCommand", [commandName, ...argValues], true);
            return JSON.stringify(result);
        },
        getViewModelSnapshot: () => {
            return JSON.stringify(dotvvm.state);
        },
        patchViewModel: (patchJson: string) => {
            dotvvm.patchState(JSON.parse(patchJson));
        }
    });

    const config = getConfig();
    const exports = await getAssemblyExports("DotVVM.Framework.Interop.DotnetWasm");
    
    interop = exports.DotVVM.Framework.Interop.DotnetWasm.DotnetWasmInterop;
}
const initPromise: Promise<void> = initDotnet();

let instanceCounter = 0;
const instanceMap: { [id: string]: string | HTMLElement } = {};

class DotnetWasmModule {
    private readonly moduleType: string;
    private readonly moduleInstanceId: string;

    constructor(public readonly context: any) {
        this.moduleType = this.context.instanceArgs![0];
        this.moduleInstanceId = "dotnet-wasm-" + (instanceCounter++);
        instanceMap[this.moduleInstanceId] = context.viewIdOrElement;
        this.init();
    }

    private async init() {
        await initPromise;
        interop.CreateViewModuleInstance(this.moduleType, this.moduleInstanceId, ["TestCommand"]);
    }

    async dotnetWasmInvoke(method: string, ...args: any[]) {
        const argValues = args.map(a => JSON.stringify(a));

        await initPromise;
        const result = await interop.CallViewModuleCommand(this.moduleType, this.moduleInstanceId, method, argValues);

        return JSON.parse(result);
    }

    async callNamedCommand(name: string, ...args: any[]) {
        const command = this.context.namedCommands[name];
        if (!command) {
            throw `NamedCommand control with name '${name}' not found.`;
        }
        return await command(...args);
    }

    async $dispose() {
        await initPromise;
        interop.DisposeViewModuleInstance(this.moduleType, this.moduleInstanceId);
        delete instanceMap[this.moduleInstanceId];
    }
}

export default (context: any) => new DotnetWasmModule(context);
