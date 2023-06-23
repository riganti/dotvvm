const eventMap: {
    [key: string]: KnockoutObservable<number>
} = {};

export function notify(id: string) {
    if (id in eventMap) {
        eventMap[id].notifySubscribers();
    }
}

export function get(id: string) {
    return eventMap[id] || (eventMap[id] = ko.observable(0).extend({ notify: 'always' }));
}
