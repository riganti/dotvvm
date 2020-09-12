// Typescript could not find the module, IDK why...
import fc_types from '../../../node_modules/fast-check/lib/types/fast-check'
import { serializeDate, parseDate } from '../serialization/date';
import { validateType } from '../serialization/typeValidation';

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

test('validateType int32', () => {
    fc.assert(fc.property(
        fc.integer(-2147483648, 2147483647),
        i => validateType(i, "int32")
    ))
})

test('validateType uint32', () => {
    fc.assert(fc.property(
        fc.integer(0, 4294967295),
        i => validateType(i, "uint32")
    ))
})

test('validateType uint64', () => {
    fc.assert(fc.property(
        fc.integer(0, 18446744073709551615),
        i => validateType(i, "uint64")
    ))
})

test('validateType int64', () => {
    fc.assert(fc.property(
        fc.integer(-9223372036854775808, 9223372036854775807),
        i => validateType(i, "int64")
    ))
})

test('validateType int64 in string', () => {
    fc.assert(fc.property(
        fc.integer(-9223372036854775808, 9223372036854775807),
        i => validateType("" + i, "int64")
    ))
})

const arbInt = fc.oneof(...["int32", "int16", "int8", "uint8", "uint16", "uint32", "uint64", "int64"].map(fc.constant))

test('validateType int does not accept decimal', () => {
    fc.assert(fc.property(
        fc.float(), arbInt,
        (num, type: any) => num % 1 == 0 || !validateType(num, type)
    ))
})

const arbFloat = fc.oneof(...["single", "double", "number", "decimal"].map(fc.constant))

test('validateType float accepts floats', () => {
    fc.assert(fc.property(
        fc.float(), arbFloat,
        (num, type: any) => validateType(num, type)
    ))
})

test('validateType float accepts floats in string', () => {
    fc.assert(fc.property(
        fc.float(), arbFloat,
        (num, type: any) => validateType("" + num, type)
    ))
})

test('validateType edge cases', () => {
    expect(validateType("", "int32")).toBeFalsy()
    expect(validateType(null, "int32")).toBeFalsy()
    expect(validateType("", "int32?")).toBeTruthy()
    expect(validateType(null, "int32?")).toBeTruthy()
    expect(validateType("anything", "unknown type ...")).toBeTruthy()
})
