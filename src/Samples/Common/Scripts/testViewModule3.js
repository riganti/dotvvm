export default (context) => new Page(context);

class Page {
    constructor(context) {
        this.context = context;
        this.appendLine("testViewModule: init");
    }
    appendLine(text) {
        let info = document.createElement("div");
        info.innerHTML = text;
        document.getElementById("log").appendChild(info)
    }
}
