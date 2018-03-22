class DotvvmSpaHistory {

    public pushPage(url: string) {
        history.pushState({ navigationType: 'SPA', url: url }, '', url);
    }

    public replacePage(url: string) {
        history.replaceState({ navigationType: 'SPA', url: url }, '', url);
    }

    public isSpaPage(state: any): boolean {
        return state && state.navigationType == 'SPA';
    }

}
