export default (context) => {

    appendLine("testViewModule2: init");

    return {
        noArgs() {
            appendLine("testViewModule2: commands.noArgs(" + serializeArgs(arguments) + ")");
        },
        oneArg(arg1) {
            appendLine("testViewModule2: commands.oneArg(" + serializeArgs(arguments) + ")");
        },
        twoArgs(arg1, arg2) {
            appendLine("testViewModule2: commands.twoArgs(" + serializeArgs(arguments) + ")");
        },
        syncIncrement(value) {
            appendLine("testViewModule2: commands.syncIncrement(" + serializeArgs(arguments) + ")");
            return value + 1;
        },
        async callIncrementCommand(value) {
            appendLine("testViewModule2: commands.callIncrementCommand(" + serializeArgs(arguments) + ")");
            return await context.namedCommands.IncrementCommand(value);
        },
        asyncIncrement(value) {
            return new Promise((resolve, reject) => {
                appendLine("testViewModule2: commands.asyncIncrement(" + serializeArgs(arguments) + ") begin");
                window.setTimeout(() => {
                    appendLine("testViewModule2: commands.asyncIncrement(" + serializeArgs(arguments) + ") end");
                    resolve(value + 1);
                }, 1000);
            });
        },
        callSetResultCommand() {
            appendLine("testViewModule2: commands.callSetResultCommand(" + serializeArgs(arguments) + ")");
            context.namedCommands.SetResultCommand(1, "test", { Test: "abc" });
        },

        $dispose() {
            appendLine("testViewModule2: dispose");
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
