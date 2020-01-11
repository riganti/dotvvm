import { createArray } from '../utils/objects'

function getParentTable(el: HTMLElement): HTMLTableElement {
    if (el instanceof HTMLTableElement) {
        return el;
    } else {
        return getParentTable(el.parentElement!);
    }
}

export default {
    'dotvvm-table-columnvisible': {
        init(element: HTMLElement, valueAccessor: () => any) {
            let lastDisplay = "";
            let currentVisible = true;
            if (!(element instanceof HTMLTableCellElement)) {
                return;
            }

            const table: any = getParentTable(element);
            const firstRow = table.rows.item(0);

            if (!firstRow) {
                throw Error("Table with dotvvm-table-columnvisible binding must not be empty.");
            }
            const colIndex = createArray(firstRow.cells).indexOf(element);

            (element as any)['dotvvmChangeVisibility'] = (visible: boolean) => {
                if (currentVisible == visible) {
                    return;
                }
                currentVisible = visible;
                for (let i = 0; i < table.rows.length; i++) {
                    const row = <HTMLTableRowElement> table.rows.item(i);
                    const style = (<HTMLElement> row.cells[colIndex]).style;
                    if (visible) {
                        style.display = lastDisplay;
                    } else {
                        lastDisplay = style.display || "";
                        style.display = "none";
                    }
                }
            }
        },
        update(element: any, valueAccessor: any) {
            element.dotvvmChangeVisibility(ko.unwrap(valueAccessor()));
        }
    },
}
