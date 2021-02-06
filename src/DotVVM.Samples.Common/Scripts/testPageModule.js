export default class {
    constructor(context) {
        this.context = context;
        this.appendLine("testPageModule: init");
    }

    $dispose() {
        this.appendLine("testPageModule: dispose");
    }

    noArgs() {
        this.appendLine("testPageModule: commands.noArgs(" + this.serializeArgs(arguments) + ")");
    }

    oneArg(arg1) {
        this.appendLine("testPageModule: commands.oneArg(" + this.serializeArgs(arguments) + ")");
    }

    twoArgs(arg1, arg2) {
        this.appendLine("testPageModule: commands.twoArgs(" + this.serializeArgs(arguments) + ")");
    }

    syncIncrement(value) {
        this.appendLine("testPageModule: commands.syncIncrement(" + this.serializeArgs(arguments) + ")");
        return value + 1;
    }

    async callIncrementCommand(value) {
        this.appendLine("testPageModule: commands.callIncrementCommand(" + this.serializeArgs(arguments) + ")");
        return await this.context.namedCommands.IncrementCommand(value);
    }

    asyncIncrement(value) {
        return new Promise((resolve, reject) => {
            this.appendLine("testPageModule: commands.asyncIncrement(" + this.serializeArgs(arguments) + ") begin");
            window.setTimeout(() => {
                this.appendLine("testPageModule: commands.asyncIncrement(" + this.serializeArgs(arguments) + ") end");
                resolve(value + 1);
            }, 1000);
        });
    }

    callSetResultCommand(context) {
        this.appendLine("testPageModule: commands.callSetResultCommand(" + this.serializeArgs(arguments) + ")");
        this.context.namedCommands.SetResultCommand(1, "test", { Test: "abc" });
    }

    appendLine(text) {
        document.getElementById("log").innerText += text + "\r\n";
    }

    serializeArgs(args) {
        let result = "";
        for (let i = 0; i < args.length; i++) {
            if (i > 0) result += ", ";
            result += JSON.stringify(args[i]);
        }
        return result;
    }
}
