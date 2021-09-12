export default context => new Page(context);

let someId1234 = 0

class Page {

    constructor(context) {
        this.context = context;
        this.state = 0;

        this.rootElement = context.elements[0].parentElement;
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

    $controls = {
        Bazmek: {
            create() {
                const id = someId1234++
                console.log("Create: ", id, [...arguments])
                return {
                    updateProps(props) {
                        console.log("UpdateProps: ", id, props)
                    },
                    dispose() {
                        console.log("Dispose: ", id, [...arguments])
                    }
                }
            }
        }
    }

    increment() {
        this.state++;
        this.rootElement.querySelector(".value").innerText = this.state;
    }

    reportState() {
        this.context.namedCommands["ReportState"](this.state);
    }

}
