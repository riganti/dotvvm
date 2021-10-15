export default context => new Page(context);

class Page {

    constructor(context) {
        this.context = context;
        this.state = 0;

        this.rootElement = context.elements[0];
        window.setTimeout(() => {
            this.rootElement.querySelector(".id").innerText = this.rootElement.id;
            this.rootElement.querySelector(".value").innerText = this.state;
        }, 0);

        dotvvm.viewModels.root.viewModel.Incrementers.subscribe(() => {
            window.setTimeout(() => {
                this.rootElement.querySelector(".id").innerText = this.rootElement.id;
            }, 0);
        });
    }

    increment() {
        this.state++;
        this.rootElement.querySelector(".value").innerText = this.state;
    }

    reportState() {
        this.context.namedCommands["ReportState"](this.state);
    }

}
