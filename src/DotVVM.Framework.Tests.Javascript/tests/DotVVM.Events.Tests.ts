/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/scripts/typings/knockout/knockout.d.ts" />
/// <reference path="./DotVVM.d.ts" />

var dotvvm = new DotVVM();

describe("DotVVM.Events", () => {
    
    it("subscribe works", () => {
        var e = new DotvvmEvent<{}>("test", false);
        var log = 0;
        var handler = a => { log++; };
        e.subscribe(handler);

        expect(log).toBe(0);

        e.trigger({});

        expect(log).toBe(1);

        e.trigger({});

        expect(log).toBe(2);
    });

    it("subscribeOnce works", () => {
        var e = new DotvvmEvent<{}>("test", false);
        var log = 0;
        var handler = a => { log++; };
        e.subscribeOnce(handler);

        expect(log).toBe(0);

        e.trigger({});

        expect(log).toBe(1);

        e.trigger({});
        
        expect(log).toBe(1);
    });

    it("unsubscribe works", () => {
        var e = new DotvvmEvent<{}>("test", false);
        var log1 = 0;
        var handler1 = a => { log1++; };
        var log2 = 0;
        var handler2 = a => { log2++; };

        e.subscribe(handler1);
        e.subscribe(handler2);

        expect(log1).toBe(0);
        expect(log2).toBe(0);

        e.trigger({});

        expect(log1).toBe(1);
        expect(log2).toBe(1);

        e.unsubscribe(handler2);
        e.trigger({});

        expect(log1).toBe(2);
        expect(log2).toBe(1);

        e.unsubscribe(handler1);
        e.trigger({});

        expect(log1).toBe(2);
        expect(log2).toBe(1);
    });

    it("subscribe with history works", () => {
        var e = new DotvvmEvent<{}>("test", true);
        var log = 0;
        var handler = a => { log++; };

        e.trigger({});
        e.trigger({});
        e.subscribe(handler);

        expect(log).toBe(2);
    });

    it("subscribe and subscribeOnce complex test", () => {
        var e = new DotvvmEvent<{}>("test", false);
        
        var log1 = 0;
        var handler1 = a => { log1++; };
        var log2 = 0;
        var handler2 = a => { log2++; };
        var log3 = 0;
        var handler3 = a => { log3++; };

        e.subscribeOnce(handler1);
        e.subscribe(handler2);
        e.subscribeOnce(handler3);
        expect(e["handlers"].length).toBe(3);

        e.trigger({});
        expect(log1).toBe(1);
        expect(log2).toBe(1);
        expect(log3).toBe(1);
        expect(e["handlers"].length).toBe(1);

        e.trigger({});
        expect(log1).toBe(1);       // it was unsubscribed automatically
        expect(log2).toBe(2);
        expect(log3).toBe(1);       // it was unsubscribed automatically

        e.unsubscribe(handler2);
        expect(e["handlers"].length).toBe(0);        
    });
    

});