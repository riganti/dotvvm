import { createArray, keys } from "../utils/objects";

export type RenderedResourceList = {
    [name: string]: string;
}

const resourceSigns: { 
    [name: string]: boolean 
} = {};

export function registerResources(rs: string[] | null | undefined) {
    if (rs)
        for (const r of rs)
            resourceSigns[r] = true;
}

export const getRenderedResources = () => keys(resourceSigns)

export async function loadResourceList(resources: RenderedResourceList | undefined) {
    if (!resources) return;

    var html = "";
    for (const name of keys(resources)) {
        if (!/^__noname_\d+$/.test(name)) {
            if (resourceSigns[name]) continue;
            resourceSigns[name] = true;
        }
        html += resources[name] + " ";
    }

    if (html.trim() == "") {
        return;
    }
    else {
        const tmp = document.createElement("div");
        tmp.innerHTML = html;
        const elements = <HTMLElement[]>createArray(tmp.children);
        await loadResourceElements(elements);
    }
}

async function loadResourceElements(elements: HTMLElement[]) {
    for (let element of elements) {
        let waitForScriptLoaded = false;
        if (element.tagName.toLowerCase() == "script") {
            let originalScript = <HTMLScriptElement>element;
            let script = <HTMLScriptElement>document.createElement("script");
            if (originalScript.src) {
                script.src = originalScript.src;
                waitForScriptLoaded = true;
            }
            if (originalScript.type) {
                script.type = originalScript.type;
            }
            if (originalScript.text) {
                script.text = originalScript.text;
            }
            if (element.id) {
                script.id = element.id;
            }
            element = script;
        }
        else if (element.tagName.toLowerCase() == "link") {
            // create link
            var originalLink = <HTMLLinkElement>element;
            var link = <HTMLLinkElement>document.createElement("link");
            if (originalLink.href) {
                link.href = originalLink.href;
            }
            if (originalLink.rel) {
                link.rel = originalLink.rel;
            }
            if (originalLink.type) {
                link.type = originalLink.type;
            }
            element = link;
        }

        // load next script when this is finished
        const loadPromise = waitForElementLoaded(element);
        document.head.appendChild(element);
        if (waitForScriptLoaded) {
            await loadPromise;
        }
    }
}

function waitForElementLoaded(element: HTMLElement) {
    return new Promise(resolve => {
        element.addEventListener("load", resolve);
        element.addEventListener("error", () => {
            console.warn(`Error loading resource`, element);
            resolve();
        });
    });
}
