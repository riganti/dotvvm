import { buildRouteUrl, buildUrlSuffix } from '../controls/routeLink'


test("buildUrlSuffix encodes", () => {
    expect(buildUrlSuffix("", { a: 1, b: "1 2" } )).toBe("?a=1&b=1%202")
    expect(buildUrlSuffix("", { a: 1, "a/b/c": 10.123 } )).toBe("?a=1&a%2Fb%2Fc=10.123")
})
test("buildUrlSuffix joins query params", () => {
    expect(buildUrlSuffix("path?q=X", { a: 1 } )).toBe("path?q=X&a=1")
    expect(buildUrlSuffix("path", { a: 1 } )).toBe("path?a=1")
    expect(buildUrlSuffix("path?q", { a: 1 } )).toBe("path?q&a=1")
})
test("buildUrlSuffix ignores null query", () => {
    expect(buildUrlSuffix("path?q=X", { a: null } )).toBe("path?q=X")
    expect(buildUrlSuffix("path?q=X", { a: "" } )).toBe("path?q=X&a=")
    expect(buildUrlSuffix("path?q=X", { a: 0 } )).toBe("path?q=X&a=0")
})
test("buildUrlSuffix handles hash", () => {
    expect(buildUrlSuffix("#myHash", { a: 1, b: 2 } )).toBe("?a=1&b=2#myHash")
    expect(buildUrlSuffix("?myQuery=2#myHash", { a: 1 } )).toBe("?myQuery=2&a=1#myHash")
})

test("buildRouteUrl encodes", () => {
    expect(buildRouteUrl("/P/{id}", { id: "1/2" })).toBe("/P/1%2F2")
})
test("buildRouteUrl handles multiple args", () => {
    expect(buildRouteUrl("/P/{id}-{name}", { id: 1, name: "N" })).toBe("/P/1-N")
    expect(buildRouteUrl("/P/a{id}b/xx-{name}", { id: 1, name: "N" })).toBe("/P/a1b/xx-N")
    expect(buildRouteUrl("/P/{id}{name}", { id: 1, name: "N" })).toBe("/P/1N")
})
test("buildRouteUrl handles optional args", () => {
    expect(buildRouteUrl("/P/{id}/{name}", { name: "N" })).toBe("/P/N")
    expect(buildRouteUrl("/P/{id}/{name}", { id: 1 })).toBe("/P/1")
    expect(buildRouteUrl("/P/a{id}/xx-{name}", { id: 1, name: null })).toBe("/P/a1")
    expect(buildRouteUrl("/P/a{id}/xx-{name}", { id: null, name: "N" })).toBe("/P/xx-N")
    expect(buildRouteUrl("/{id}", { id: null })).toBe("")
})
