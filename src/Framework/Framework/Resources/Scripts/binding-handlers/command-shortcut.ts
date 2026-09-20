type CommandShortcutProps = {
    command: () => Promise<unknown>,
    key: string | KnockoutObservable<string>,
    ctrl: boolean | KnockoutObservable<boolean>,
    shift: boolean | KnockoutObservable<boolean>,
    alt: boolean | KnockoutObservable<boolean>,
    enabled: boolean | KnockoutObservable<boolean>,
    targetId?: string
}

ko.virtualElements.allowedBindings["dotvvm-command-shortcut"] = true;

const keyCodes: Record<string, number> = {
    None: 0,
    Backspace: 8,
    Tab: 9,
    Enter: 13,
    Pause: 19,
    CapsLock: 20,
    Escape: 27,
    Space: 32,
    PageUp: 33,
    PageDown: 34,
    End: 35,
    Home: 36,
    Left: 37,
    Up: 38,
    Right: 39,
    Down: 40,
    Insert: 45,
    Delete: 46,
    Multiply: 106,
    Add: 107,
    Subtract: 109,
    DecimalPoint: 110,
    Divide: 111
};

function getKeyCode(key: string) {
    if (keyCodes[key] !== undefined) {
        return keyCodes[key];
    }
    if (/^[A-Z]$/.test(key)) {
        return key.charCodeAt(0);
    }
    if (/^D[0-9]$/.test(key)) {
        return key.charCodeAt(1);
    }
    const numPad = /^NumPad([0-9])$/.exec(key);
    if (numPad) {
        return 96 + Number(numPad[1]);
    }
    const functionKey = /^F([1-9]|1[0-2])$/.exec(key);
    return functionKey ? 111 + Number(functionKey[1]) : -1;
}

export default {
    "dotvvm-command-shortcut": {
        init(element: Node, valueAccessor: () => CommandShortcutProps) {
            const target = valueAccessor().targetId
                ? document.getElementById(valueAccessor().targetId!)
                : document;

            if (!target) {
                throw new Error(`The CommandShortcut target '${valueAccessor().targetId}' was not found.`);
            }

            const handler = async (event: Event) => {
                const keyboardEvent = event as KeyboardEvent;
                const props = valueAccessor();
                if (ko.unwrap(props.enabled)
                    && keyboardEvent.keyCode === getKeyCode(ko.unwrap(props.key))
                    && keyboardEvent.ctrlKey === ko.unwrap(props.ctrl)
                    && keyboardEvent.shiftKey === ko.unwrap(props.shift)
                    && keyboardEvent.altKey === ko.unwrap(props.alt)) {
                    keyboardEvent.preventDefault();
                    try {
                        await props.command.call(target);
                    } catch (err) {
                        dotvvm.log.logError("postback", err);
                    }
                }
            };

            target.addEventListener("keydown", handler);
            ko.utils.domNodeDisposal.addDisposeCallback(element, () =>
                target.removeEventListener("keydown", handler));
        }
    }
};
