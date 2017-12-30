(function () {
    if (typeof Promise === 'undefined' || !self.fetch) {
        var resource = document.createElement('script');
        resource.src = window['dotvvm__polyfillUrl'];
        resource.type = "text/javascript";

        var headElement = <HTMLElement>document.getElementsByTagName('head')[0];
        headElement.appendChild(resource);
    }
})();
