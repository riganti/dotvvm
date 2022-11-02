import { parseDate, parseDateOnly, parseDateTimeOffset, parseTimeOnly, parseTimeSpan, serializeDateOnly, serializeTimeOnly } from "../serialization/date"

test("Date invalid", () => {
    expect(parseDate("")).toBe(null);
    expect(parseDate("2021-01-40T12:12:12")).toBe(null);
    expect(parseDate("2020-01-01T12:12:12.")).toBe(null);
    expect(parseDate("2020-01-01T12:12:12.grrrr")).toBe(null);
    expect(parseDate("blablabla2020-01-01T12:12:12")).toBe(null);
})

test("Date 6:58:10.500001 PM 8/10/2022", () => {
    testDateIsSameAfterParse("2022-08-10T18:58:10.501000");
});

test("Date 8/10/2022 - date only", () => {
    testDateIsSameAfterParse("2022-08-10T00:00:00.000000");
});

test("Date 01:01:01.1 01/01/1901", () => {
    testDateIsSameAfterParse("1901-01-01T01:01:01.100000");
});

test("Date 6:58:10 PM 0/10/2022 - Zero month", () => {
    //No practical way to set day 0 and month 0 to JS date object
    testDateIsSameAfterParse("2022-00-10T18:58:10.000000", "2022-01-10T18:58:10.000000");
});

test("Date 6:58:10 PM 5/0/2022 - Zero day", () => {
    //No practical way to set day 0 and month 0 to JS date object
    testDateIsSameAfterParse("2022-05-00T18:58:10.000000", "2022-05-01T18:58:10.000000");
});

test("Date 6:58:10 PM 0/0/2022 - Zero day and month", () => {
    //No practical way to set day 0 and month 0 to JS date object
    testDateIsSameAfterParse("2022-00-00T18:58:10.000000", "2022-01-01T18:58:10.000000");
});

test("Date 6:58:10 PM 1/1/0 - Zero year", () => {
    testDateIsSameAfterParse("0000-01-01T18:58:10.000000");
});

test("Date 0:00:00 PM 0/0/0 - Zero all", () => {
    //No practical way to set day 0 and month 0 to JS date object
    testDateIsSameAfterParse("0000-00-00T00:00:00.000000", "0000-01-01T00:00:00.000000"); 
});

test("Date 0:00:00 PM 1/1/1 - Year one", () => {
    testDateIsSameAfterParse("0001-01-01T00:00:00.000000");
});

test("Date 0:00:00 PM 1/1/10 - Year ten", () => {
    testDateIsSameAfterParse("0010-01-01T00:00:00.000000");
});

test("Date 0:00:00 PM 1/1/100 - Year hunderd", () => {
    testDateIsSameAfterParse("0100-01-01T00:00:00.000000");
});

test("Date 0:00:00 PM 1/1/1000 - Year thousand", () => {
    testDateIsSameAfterParse("1000-01-01T00:00:00.000000");
});

test("Date 0:00:00 PM 1/1/9999 - Year 9999", () => {
    testDateIsSameAfterParse("9999-01-01T00:00:00.000000");
});

test("Date 6/28/2021 3:28:31 PM", () => {
    testDateIsSameAfterParse("2021-06-28T15:28:31.000000");
});

function testDateIsSameAfterParse(dateInputString: string, expectedOutputDateString: string|null = null) {
    const date = parseDate(dateInputString);

    const outputDateString = date ? dotvvm_Globalize.format(date, "yyyy-MM-ddTHH:mm:ss.fff000") : null;
    expectedOutputDateString = expectedOutputDateString ?? dateInputString;

    expect(outputDateString).toBe(expectedOutputDateString);
}


test("date parse null propagation", () => {
    expect(parseDate(null)).toBe(null)
    expect(parseDate(undefined)).toBe(null)
    expect(parseDateOnly(null)).toBe(null)
    expect(parseDateOnly(undefined)).toBe(null)
    expect(parseTimeOnly(null)).toBe(null)
    expect(parseTimeOnly(undefined)).toBe(null)
    expect(parseTimeSpan(null)).toBe(null)
    expect(parseTimeSpan(undefined)).toBe(null)
    expect(parseDateTimeOffset(null)).toBe(null)
    expect(parseDateTimeOffset(undefined)).toBe(null)
})


test("DateTimeOffset parse", () => {
    const d = parseDateTimeOffset("2021-06-28T15:28:31.000000+02:00")
    // we only know the UTC time of d, toLocalString() would be in local time of whoever is running the test
    expect(d?.toISOString()).toBe("2021-06-28T13:28:31.000Z")
})
test("DateTimeOffset invalid", () => {
    expect(parseDateTimeOffset("")).toBe(null)
    expect(parseDateTimeOffset("2021-01-40T15:28:31.000000+02:00")).toBe(null)
})

test("DateOnly serialize", () => {
    expect(serializeDateOnly(new Date(2020, 1, 29))).toBe("2020-02-29")
})

test("DateOnly parse", () => {
    const d = parseDateOnly("2020-01-10")
    expect(d?.toLocaleString('sv')).toBe("2020-01-10 00:00:00")

    expect(serializeDateOnly(d!)).toBe("2020-01-10")
})

test("TimeOnly serialize", () => {
    expect(serializeTimeOnly(new Date(0, 0, 1, 10, 10, 10, 123))).toBe("10:10:10.1230000")
})

test("TimeOnly parse", () => {
    const d = parseTimeOnly("18:58:10.501")
    expect(d?.toLocaleTimeString('sv')).toBe("18:58:10")
    expect(d?.getHours()).toBe(18)
    expect(d?.getMilliseconds()).toBe(501)

    expect(serializeTimeOnly(d!)).toBe("18:58:10.5010000")
})
