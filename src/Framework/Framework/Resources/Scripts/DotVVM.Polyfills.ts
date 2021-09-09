export default function() {
    if (typeof Promise === 'undefined' || !self.fetch) {
        const resource = document.createElement('script');
        resource.src = (<any> window)['dotvvm__polyfillUrl'];
        resource.type = "text/javascript";

        const headElement = <HTMLElement> document.getElementsByTagName('head')[0];
        headElement.appendChild(resource);
    }
};
