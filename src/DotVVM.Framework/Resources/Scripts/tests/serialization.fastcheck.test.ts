// Typescript could not find the module, IDK why...
import fc_types from '../../../node_modules/fast-check/lib/types/fast-check'
import { tryCoerce } from '../metadata/coercer';
import { serializeDate, parseDate } from '../serialization/date';

const fc: typeof fc_types = require('fast-check');

const reasonableDate = fc.date({ min: new Date(1800, 1, 1, 0, 0, 0, 0), max: new Date(2200, 1, 1, 0, 0, 0, 0) })

test('Serialize and parse date', () => {
    fc.assert(fc.property(
        reasonableDate,
        date => {
            const serialized = serializeDate(date, false)
            const parsedDate = parseDate(serialized!)
            expect(date).toStrictEqual(parsedDate)

            // hmm, actually, I don't know why this works 🤔
            const normalParsed = new Date(serialized!)
            expect(normalParsed).toStrictEqual(date)

            expect(serializeDate(serialized, false)).toBe(serialized)
        }
    ))

    // it should just pass null
    expect(serializeDate(null)).toBeNull()
})

test("Parse date never throws", () => {
    fc.assert(fc.property(
        fc.string(),
        x => {
            parseDate(x)
            return true
        }
    ))
})

test('tryCoerce Int32', () => {
    fc.assert(fc.property(
        fc.integer(-2147483648, 2147483647),
        i => typeof tryCoerce(i, "Int32") === "object"
    ))
})

test('tryCoerce UInt32', () => {
    fc.assert(fc.property(
        fc.integer(0, 4294967295),
        i => typeof tryCoerce(i, "UInt32") === "object"
    ))
})

test('tryCoerce UInt64', () => {
    fc.assert(fc.property(
        fc.integer(0, 18446744073709551615),
        i => typeof tryCoerce(i, "UInt64") === "object"
    ))
})

test('tryCoerce Int64', () => {
    fc.assert(fc.property(
        fc.integer(-9223372036854775808, 9223372036854775807),
        i => typeof tryCoerce(i, "Int64") === "object"
    ))
})

test('tryCoerce Int64 in string', () => {
    fc.assert(fc.property(
        fc.integer(-9223372036854775808, 9223372036854775807),
        i => typeof tryCoerce("" + i, "Int64") === "object"
    ))
})

const arbInt = fc.oneof(...["Int32", "Int16", "SByte", "Byte", "UInt16", "UInt32", "UInt64", "Int64"].map(fc.constant))

test('tryCoerce int does not accept decimal', () => {
    fc.assert(fc.property(
        fc.float(), arbInt,
        (num, type: TypeDefinition) => num % 1 == 0 || tryCoerce(num, type)!.wasCoerced      // TODO: coercion now rounds the value - do we want it?
    ))
})

const arbFloat = fc.oneof(...["Single", "Double", "Decimal"].map(fc.constant))

test('tryCoerce float accepts floats', () => {
    fc.assert(fc.property(
        fc.float(), arbFloat,
        (num, type: TypeDefinition) => typeof tryCoerce(num, type) === "object"
    ))
})

test('tryCoerce float accepts floats in string', () => {
    fc.assert(fc.property(
        fc.float(), arbFloat,
        (num, type: TypeDefinition) => typeof tryCoerce("" + num, type) === "object"
    ))
})

test('tryCoerce edge cases', () => {
    expect(tryCoerce("", "Int32")).toBeFalsy()
    expect(tryCoerce(null, "Int32")).toBeFalsy()
    expect(tryCoerce("", { type: "nullable", inner: "Int32" })).toBeTruthy()
    expect(tryCoerce(null, { type: "nullable", inner: "Int32" })).toBeTruthy()
    expect(() => tryCoerce("anything", "unknown type ...")).toThrow()
})
