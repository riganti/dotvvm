export default context => new MyModule(context);

class MyModule {

    constructor(context) {
        this.context = context;
    }

    callCommand(v) {
        this.context.namedCommands["test"](v);
    }

}
