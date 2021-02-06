export default (context) => {

    appendLine("testPageModule2: init");

    return {
        noArgs() {
            appendLine("testPageModule2: commands.noArgs(" + serializeArgs(arguments) + ")");
        },
        oneArg(arg1) {
            appendLine("testPageModule2: commands.oneArg(" + serializeArgs(arguments) + ")");
        },
        twoArgs(arg1, arg2) {
            appendLine("testPageModule2: commands.twoArgs(" + serializeArgs(arguments) + ")");
        },
        syncIncrement(value) {
            appendLine("testPageModule2: commands.syncIncrement(" + serializeArgs(arguments) + ")");
            return value + 1;
        },
        async callIncrementCommand(value) {
            appendLine("testPageModule2: commands.callIncrementCommand(" + serializeArgs(arguments) + ")");
            return await context.namedCommands.IncrementCommand(value);
        },
        asyncIncrement(value) {
            return new Promise((resolve, reject) => {
                appendLine("testPageModule2: commands.asyncIncrement(" + serializeArgs(arguments) + ") begin");
                window.setTimeout(() => {
                    appendLine("testPageModule2: commands.asyncIncrement(" + serializeArgs(arguments) + ") end");
                    resolve(value + 1);
                }, 1000);
            });
        },
        callSetResultCommand() {
            appendLine("testPageModule2: commands.callSetResultCommand(" + serializeArgs(arguments) + ")");
            context.namedCommands.SetResultCommand(1, "test", { Test: "abc" });
        },

        $dispose() {
            appendLine("testPageModule2: dispose");
        }
    }

    function appendLine(text) {
        document.getElementById("log").innerText += text + "\r\n";
    }

    function serializeArgs(args) {
        let result = "";
        for (let i = 0; i < args.length; i++) {
            if (i > 0) result += ", ";
            result += JSON.stringify(args[i]);
        }
        return result;
    }

}
