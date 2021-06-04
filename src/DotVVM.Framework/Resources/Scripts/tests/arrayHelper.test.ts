import * as arrayHelper from '../collections/arrayHelper'
import { getStateManager } from '../dotvvm-base'
import { StateManager } from '../state-manager'
require('./stateManagement.data')

const vm = dotvvm.viewModels.root.viewModel as any
const s = getStateManager() as StateManager<any>
s.doUpdateNow()

test("Initial knockout ViewModel", () => {
    expect(vm.Array).observableArray()
    expect(vm.Array()[0]).observable()
    expect(vm.Array()[0]().Id).observable()
    expect(vm.Array()[0]().Id()).toBe(1)
})

test("List::Add", () => {
    arrayHelper.add(vm.Array, { Id: 2 });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(2);
    expect(vm.Array()[1]().Id()).toBe(2);
})

test("List::Clear", () => {
    arrayHelper.clear(vm.Array);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(0);
})

test("List::AddRange", () => {
    prepareArray();
    arrayHelper.addRange(vm.Array, [{ Id: 6 }, { Id: 7 }]);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(7);
    expect(vm.Array()[5]().Id()).toBe(6);
    expect(vm.Array()[6]().Id()).toBe(7);
})

test("List::Insert", () => {
    prepareArray();
    arrayHelper.insert(vm.Array, 1, { Id: 123 });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(6);
    expect(vm.Array()[0]().Id()).toBe(1);
    expect(vm.Array()[1]().Id()).toBe(123);
    expect(vm.Array()[2]().Id()).toBe(2);
})

test("List::InsertRange", () => {
    prepareArray();
    arrayHelper.insertRange(vm.Array, 1, [{ Id: 123 }, { Id: 321 }]);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(7);
    expect(vm.Array()[0]().Id()).toBe(1);
    expect(vm.Array()[1]().Id()).toBe(123);
    expect(vm.Array()[2]().Id()).toBe(321);
    expect(vm.Array()[3]().Id()).toBe(2);
})

test("List::RemoveAt", () => {
    prepareArray();
    arrayHelper.removeAt(vm.Array, 1);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(4);
    expect(vm.Array()[0]().Id()).toBe(1);
    expect(vm.Array()[1]().Id()).toBe(3);
})

test("List::RemoveRange", () => {
    prepareArray();
    arrayHelper.removeRange(vm.Array, 1, 2);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(3);
    expect(vm.Array()[0]().Id()).toBe(1);
    expect(vm.Array()[1]().Id()).toBe(4);
})

test("List::Reverse", () => {
    prepareArray();
    arrayHelper.reverse(vm.Array);
    s.doUpdateNow();

    expect(vm.Array().length).toBe(5);
    expect(vm.Array()[0]().Id()).toBe(5);
    expect(vm.Array()[1]().Id()).toBe(4);
    expect(vm.Array()[2]().Id()).toBe(3);
    expect(vm.Array()[3]().Id()).toBe(2);
    expect(vm.Array()[4]().Id()).toBe(1);
})

test("ListExtensions::AddOrUpdate with non-existing element", () => {
    prepareArray();
    arrayHelper.addOrUpdate(vm.Array, { Id: 123 },
        function (arg) { return ko.unwrap(ko.unwrap(arg).Id) == 123 },
        function (arg) { return { Id: 321 }; });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(6);
    expect(vm.Array()[5]().Id()).toBe(123);
})

test("ListExtensions::AddOrUpdate with existing element", () => {
    prepareArray();
    arrayHelper.addOrUpdate(vm.Array, { Id: 123 },
        function (arg) { return ko.unwrap(ko.unwrap(arg).Id) == 2 },
        function (arg) { return { Id: 321 }; });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(5);
    expect(vm.Array()[0]().Id()).toBe(1);
    expect(vm.Array()[1]().Id()).toBe(321);
    expect(vm.Array()[2]().Id()).toBe(3);
})

test("ListExtensions::RemoveFirst", () => {
    prepareArray();
    arrayHelper.removeFirst(vm.Array, function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 !== 0 });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(4);
    expect(vm.Array()[0]().Id()).toBe(2);
    expect(vm.Array()[1]().Id()).toBe(3);
    expect(vm.Array()[2]().Id()).toBe(4);
})

test("ListExtensions::RemoveLast", () => {
    prepareArray();
    arrayHelper.removeLast(vm.Array, function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 !== 0 });
    s.doUpdateNow();

    expect(vm.Array().length).toBe(4);
    expect(vm.Array()[2]().Id()).toBe(3);
    expect(vm.Array()[3]().Id()).toBe(4);
})

test("Enumerable::All", () => {
    prepareArray();
    expect(arrayHelper.all(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 !== 0 })).toBe(false);
    expect(arrayHelper.all(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 === 0 })).toBe(false);
    expect(arrayHelper.all(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) <= 5 })).toBe(true);
})

test("Enumerable::Any", () => {
    prepareArray();
    expect(arrayHelper.any(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) > 10 })).toBe(false);
    expect(arrayHelper.any(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) < 10 })).toBe(true);
})

test("Enumerable::FirstOrDefault", () => {
    prepareArray();
    expect(arrayHelper.firstOrDefault(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 !== 0 })().Id()).toBe(1);
    expect(arrayHelper.firstOrDefault(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 === 0 })().Id()).toBe(2);
})

test("Enumerable::LastOrDefault", () => {
    prepareArray();
    expect(arrayHelper.lastOrDefault(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 !== 0 })().Id()).toBe(5);
    expect(arrayHelper.lastOrDefault(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) % 2 === 0 })().Id()).toBe(4);
})

test("Enumerable::Max", () => {
    prepareArray();
    expect(arrayHelper.max(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, true)).toBe(5);
    expect(arrayHelper.max(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, false)).toBe(5);

    arrayHelper.clear(vm.Array);
    s.doUpdateNow();
    expect(() => arrayHelper.max(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, true)).toThrowError("Source is empty! Max operation cannot be performed.");
    expect(arrayHelper.max(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, false)).toBe(null);
})

test("Enumerable::Min", () => {
    prepareArray();
    expect(arrayHelper.min(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, true)).toBe(1);
    expect(arrayHelper.min(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, false)).toBe(1);

    arrayHelper.clear(vm.Array);
    s.doUpdateNow();
    expect(() => arrayHelper.min(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, true)).toThrowError("Source is empty! Min operation cannot be performed.");
    expect(arrayHelper.min(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, false)).toBe(null);
})

test("Enumerable::OrderBy", () => {
    prepareArray();
    const result = arrayHelper.orderBy(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, "t2");
    expect(result.length).toBe(5);
    expect(result[0]().Id()).toBe(1);
    expect(result[1]().Id()).toBe(2);
    expect(result[2]().Id()).toBe(3);
    expect(result[3]().Id()).toBe(4);
    expect(result[4]().Id()).toBe(5);
})

test("Enumerable::OrderByDescending", () => {
    prepareArray();
    const result = arrayHelper.orderByDesc(vm.Array(), function (arg: any) { return ko.unwrap(ko.unwrap(arg).Id) }, "t2");
    expect(result.length).toBe(5);
    expect(result[0]().Id()).toBe(5);
    expect(result[1]().Id()).toBe(4);
    expect(result[2]().Id()).toBe(3);
    expect(result[3]().Id()).toBe(2);
    expect(result[4]().Id()).toBe(1);
})

function prepareArray() {
    arrayHelper.clear(vm.Array);
    arrayHelper.add(vm.Array, { Id: 1 });
    arrayHelper.add(vm.Array, { Id: 2 });
    arrayHelper.add(vm.Array, { Id: 3 });
    arrayHelper.add(vm.Array, { Id: 4 });
    arrayHelper.add(vm.Array, { Id: 5 });
    s.doUpdateNow();
}
