class HistoryRecord {
    constructor(public navigationType: string, public url: string) { }
}

class DotvvmSpaHistory {

    public pushPage(url: string) {
        // pushState doesn't work when the url is empty
        url = url || "/";

        history.pushState(new HistoryRecord('SPA', url), '', url);
    }

    public replacePage(url: string) {
        history.replaceState(new HistoryRecord('SPA', url), '', url);
    }

    public isSpaPage(state: any): boolean {
        return state && state.navigationType == 'SPA';
    }

    public getHistoryRecord(state: any): HistoryRecord {
        return <HistoryRecord>state;
    }
}
