export default (context) => new Page(context);

class Page {
    constructor(context) {
        this.context = context;
        this.appendLine("testViewModule: init");
    }

    $dispose() {
        this.appendLine("testViewModule: dispose");
    }

    noArgs() {
        this.appendLine("testViewModule: commands.noArgs(" + this.serializeArgs(arguments) + ")");
    }

    oneArg(arg1) {
        this.appendLine("testViewModule: commands.oneArg(" + this.serializeArgs(arguments) + ")");
    }

    twoArgs(arg1, arg2) {
        this.appendLine("testViewModule: commands.twoArgs(" + this.serializeArgs(arguments) + ")");
    }

    syncIncrement(value) {
        this.appendLine("testViewModule: commands.syncIncrement(" + this.serializeArgs(arguments) + ")");
        return value + 1;
    }

    async callIncrementCommand(value) {
        this.appendLine("testViewModule: commands.callIncrementCommand(" + this.serializeArgs(arguments) + ")");
        return await this.context.namedCommands.IncrementCommand(value);
    }

    asyncIncrement(value) {
        return new Promise((resolve, reject) => {
            this.appendLine("testViewModule: commands.asyncIncrement(" + this.serializeArgs(arguments) + ") begin");
            window.setTimeout(() => {
                this.appendLine("testViewModule: commands.asyncIncrement(" + this.serializeArgs(arguments) + ") end");
                resolve(value + 1);
            }, 1000);
        });
    }

    callSetResultCommand(context) {
        this.appendLine("testViewModule: commands.callSetResultCommand(" + this.serializeArgs(arguments) + ")");
        this.context.namedCommands.SetResultCommand(1, "test", { Test: "abc" });
    }

    appendLine(text) {
        document.getElementById("log").innerText += text + "\r\n";
    }

    serializeArgs(args) {
        let result = "";
        for (let i = 0; i < args.length; i++) {
            if (i > 0) result += ", ";
            result += JSON.stringify(args[i], Object.keys(args[i]).sort().reverse());
        }
        return result;
    }

    serializeArgsTest(arg1, arg2) {
        return this.serializeArgs([arg1, arg2]);
    }

    buildQuery(page, queryParameters) {
        if (!queryParameters || !queryParameters.length) { queryParameters = []; }

        let queryParamObject = this.composeQueryParametersObject(queryParameters);

        return dotvvm.buildUrlSuffix(page, queryParamObject);
    }

    composeQueryParametersObject(queryParameters) {
        let queryParamObject = {};
        queryParameters.map(qp => { queryParamObject[qp.Key] = qp.Value; });
        return queryParamObject;
    }

    readControlProperty() {
        return this.context.properties.ControlProperty.state
    }
}
