class DotvvmDomUtils {

    public onDocumentReady(callback: () => void) {
        // many thanks to http://dustindiaz.com/smallest-domready-ever
        /in/.test(document.readyState) ? setTimeout('dotvvm.domUtils.onDocumentReady(' + callback + ')', 9) : callback();
    }

    public attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture: boolean = false) {
        if (target.addEventListener) {
            target.addEventListener(name, callback, useCapture);
        }
        else {
            target.attachEvent("on" + name, callback);
        }
    }

}