import commandShortcut from "../binding-handlers/command-shortcut";

const handler = commandShortcut["dotvvm-command-shortcut"];

function keydown(target: Document | HTMLElement, options: KeyboardEventInit & { keyCode: number }) {
    const event = new KeyboardEvent("keydown", options);
    Object.defineProperty(event, "keyCode", { value: options.keyCode });
    target.dispatchEvent(event);
    return event;
}

test("document shortcut matches key and modifiers", () => {
    const command = jest.fn(() => Promise.resolve());
    const marker = document.createComment("shortcut");
    document.body.appendChild(marker);
    handler.init(marker, () => ({
        command,
        key: "S",
        ctrl: true,
        shift: true,
        alt: false,
        enabled: true
    }));

    const event = keydown(document, { keyCode: 83, ctrlKey: true, shiftKey: true, cancelable: true });
    expect(command).toHaveBeenCalledTimes(1);
    expect(event.defaultPrevented).toBe(true);

    keydown(document, { keyCode: 83, ctrlKey: true });
    expect(command).toHaveBeenCalledTimes(1);

    ko.removeNode(marker);
    keydown(document, { keyCode: 83, ctrlKey: true, shiftKey: true });
    expect(command).toHaveBeenCalledTimes(1);
});

test("targeted shortcut only handles events from the target", () => {
    document.body.innerHTML = "<div id='scope'><input id='inside'></div><input id='outside'>";
    const command = jest.fn(() => Promise.resolve());
    const marker = document.createComment("shortcut");
    document.body.appendChild(marker);
    handler.init(marker, () => ({
        command,
        key: "Enter",
        ctrl: true,
        shift: false,
        alt: false,
        enabled: true,
        targetId: "scope"
    }));

    keydown(document.getElementById("outside")!, { keyCode: 13, ctrlKey: true, bubbles: true });
    expect(command).not.toHaveBeenCalled();

    keydown(document.getElementById("inside")!, { keyCode: 13, ctrlKey: true, bubbles: true });
    expect(command).toHaveBeenCalledTimes(1);
});

test("disabled shortcut does not invoke command", () => {
    const command = jest.fn(() => Promise.resolve());
    const marker = document.createComment("shortcut");
    document.body.appendChild(marker);
    handler.init(marker, () => ({
        command,
        key: "Escape",
        ctrl: false,
        shift: false,
        alt: false,
        enabled: ko.observable(false)
    }));

    keydown(document, { keyCode: 27 });
    expect(command).not.toHaveBeenCalled();
});

test("same-scope shortcuts all run and targeted shortcuts do not propagate", () => {
    document.body.innerHTML = "<div id='scope'><input id='inside'></div><input id='outside'>";
    const targetCommands = [jest.fn(() => Promise.resolve()), jest.fn(() => Promise.resolve())];
    const documentCommands = [jest.fn(() => Promise.resolve()), jest.fn(() => Promise.resolve())];
    const markers = [...targetCommands.map(command => ({ command, targetId: "scope" })), ...documentCommands.map(command => ({ command }))].map(props => {
        const marker = document.createComment("shortcut");
        document.body.appendChild(marker);
        handler.init(marker, () => ({
            ...props,
            key: "K",
            ctrl: false,
            shift: false,
            alt: false,
            enabled: true
        }));
        return marker;
    });

    keydown(document.getElementById("inside")!, { keyCode: 75, bubbles: true });
    expect(targetCommands[0]).toHaveBeenCalledTimes(1);
    expect(targetCommands[1]).toHaveBeenCalledTimes(1);
    expect(documentCommands[0]).not.toHaveBeenCalled();
    expect(documentCommands[1]).not.toHaveBeenCalled();

    keydown(document.getElementById("outside")!, { keyCode: 75, bubbles: true });
    expect(documentCommands[0]).toHaveBeenCalledTimes(1);
    expect(documentCommands[1]).toHaveBeenCalledTimes(1);

    markers.forEach(marker => ko.removeNode(marker));
});
