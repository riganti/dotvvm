import { parseDate } from "../serialization/date"

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

function testDateIsSameAfterParse(dateInputString: string, expectedOutputDateString: string|null = null) {
    const date = parseDate(dateInputString) ?? new Date();
    const outputDateString = dotvvm_Globalize.format(date, "yyyy-MM-ddTHH:mm:ss.fff000");
    expectedOutputDateString = expectedOutputDateString ?? dateInputString;

    expect(outputDateString).toBe(expectedOutputDateString);
}
