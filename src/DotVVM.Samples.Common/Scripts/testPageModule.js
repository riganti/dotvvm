
export function init(context) {
    console.info('Hello from the init');
    console.info(context);
}

export function dispose(context) {
    console.info('Hello from the dispose');
    console.info(context);
}

export const commands = {
    testMe: function (context) {
        console.info("Hello from the command");
        console.info(context);
    }
}

