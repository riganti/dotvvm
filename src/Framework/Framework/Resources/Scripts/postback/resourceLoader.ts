import { logWarning } from "../utils/logging";
import { createArray, keys } from "../utils/objects";

export type RenderedResourceList = {
    [name: string]: string;
}

const resourceSigns: { 
    [name: string]: boolean 
} = {};

const moduleLoaderResolvers: ((value: unknown) => void)[] = [];

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
    let modulePromises: Promise<unknown>[] = [];
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

            if (script.type == "module" && !script.src) {
                let promiseId = moduleLoaderResolvers.length;
                script.text += "\n;dotvvm.resourceLoader.notifyModuleLoaded(" + promiseId + ");";
                
                let promise = new Promise((resolve, reject) => {
                    moduleLoaderResolvers[promiseId] = resolve;
                });
                modulePromises.push(promise);
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

        // wait for all modules to be loaded
        await Promise.all(modulePromises);
    }
}

function waitForElementLoaded(element: HTMLElement) {
    return new Promise<void>(resolve => {
        element.addEventListener("load", () => resolve());
        element.addEventListener("error", () => {
            logWarning("resource-loader", `Error loading resource`, element);
            resolve();
        });
    });
}

export function notifyModuleLoaded(id: number) {
    moduleLoaderResolvers[id](void 0);
    delete moduleLoaderResolvers[id];
}
