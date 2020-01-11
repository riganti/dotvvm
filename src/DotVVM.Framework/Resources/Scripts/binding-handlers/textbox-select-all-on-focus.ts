export default {
    "dotvvm-textbox-select-all-on-focus": {
        init(element: any) {
            element.$selectAllOnFocusHandler = () => {
                element.select();
            };
        },
        update(element: any, valueAccessor: () => any) {
            const value = ko.unwrap(valueAccessor());

            if (value === true) {
                element.addEventListener("focus", element.$selectAllOnFocusHandler);
            } else {
                element.removeEventListener("focus", element.$selectAllOnFocusHandler);
            }
        }
    }
}
