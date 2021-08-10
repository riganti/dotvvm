using Newtonsoft.Json.Linq;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement.ClientGlobalize
{
    public static class JQueryGlobalizeScriptCreator
    {
        private static readonly string[] numberPatternStrings =
        { "(n)", "-n", "- n", "n-", "n -" };
        private static readonly string[] percentPositivePatternStrings =
        { "n %", "n%", "%n", "% n" };
        private static readonly string[] percentNegativePatternStrings =
        { "-n %", "-n%", "-%n", "%-n", "%n", "n-%", "n%-", "-% n", "n %-", "% n-", "% -n", "n- %"};
        private static readonly string[] currencyNegativePatternStrings =
        { "($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)" };
        private static readonly string[] currencyPositivePatternStrings =
        { "$n", "n$", "$ n", "n $" };
        private static readonly JObject defaultJson = JObject.Parse(@"{
	name: 'en',
    englishName: 'English',
    nativeName: 'English',
    isRTL: false,
    language: 'en',
    numberFormat: {
        pattern: ['-n'],
        decimals: 2,
        ',': ',',
        '.': '.',
        groupSizes: [3],
        '+': '+',
        '-': '-',
        NaN: 'NaN',
        negativeInfinity: '-Infinity',
        positiveInfinity: 'Infinity',
        percent: {
            pattern: ['-n %', 'n %'],
            decimals: 2,
            groupSizes: [3],
            ',': ',',
            '.': '.',
            symbol: '%'
        },
		currency: {
			pattern: [ '($n)', '$n' ],
			decimals: 2,
			groupSizes: [ 3 ],
			',': ',',
			'.': '.',
			symbol: '$'
		}
	},
	calendars: {
		standard: {
			name: 'Gregorian_USEnglish',
			'/': '/',
			':': ':',
			firstDay: 0,
			days: {
				names: [ 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday' ],
				namesAbbr: [ 'Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat' ],
				namesShort: [ 'Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa' ]
			},
			months: {
				names: [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', '' ],
				namesAbbr: [ 'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', '' ]
			},
			AM: [ 'AM', 'am', 'AM' ],
			PM: [ 'PM', 'pm', 'PM' ],
			eras: [
				{
					'name': 'A.D.',
					'start': null,
					'offset': 0
				}
			],
			twoDigitYearMax: 2029,
			'patterns': {
				'd': 'M/d/yyyy',
				'D': 'dddd, MMMM dd, yyyy',
				't': 'h:mm tt',
				'T': 'h:mm:ss tt',
				'f': 'dddd, MMMM dd, yyyy h:mm tt',
				'F': 'dddd, MMMM dd, yyyy h:mm:ss tt',
				'M': 'MMMM dd',
				'Y': 'yyyy MMMM',
				'S': 'yyyy\u0027-\u0027MM\u0027-\u0027dd\u0027T\u0027HH\u0027:\u0027mm\u0027:\u0027ss'
			}
		}
	},
	'messages': {}
}
");

        private static JObject CreateNumberInfoJson(NumberFormatInfo ni)
        {
            var numberFormat = new
            {
                pattern = new[] { numberPatternStrings[ni.NumberNegativePattern] },
                decimals = ni.NumberDecimalDigits,
                groupSizes = ni.NumberGroupSizes,
                NaN = ni.NaNSymbol,
                negativeInfinity = ni.NegativeInfinitySymbol,
                positiveInfinity = ni.PositiveInfinitySymbol,
                percent = new
                {
                    pattern = new[] {
                            percentNegativePatternStrings[ni.PercentNegativePattern],
                            percentPositivePatternStrings[ni.PercentPositivePattern]
                        },
                    decimals = ni.PercentDecimalDigits,
                    groupSizes = ni.PercentGroupSizes,
                    symbol = ni.PercentSymbol
                },
                currency = new
                {
                    pattern = new[]
                        {
                            currencyNegativePatternStrings[ni.CurrencyNegativePattern],
                            currencyPositivePatternStrings[ni.CurrencyPositivePattern]
                        },
                    decimals = ni.CurrencyDecimalDigits,
                    groupSizes = ni.CurrencyGroupSizes,
                    symbol = ni.CurrencySymbol
                }
            };
            var jobj = JObject.FromObject(numberFormat);
            jobj[","] = ni.NumberGroupSeparator;
            jobj["."] = ni.NumberDecimalSeparator;
            jobj["percent"][","] = ni.PercentGroupSeparator;
            jobj["percent"]["."] = ni.PercentDecimalSeparator;

            jobj["currency"][","] = ni.CurrencyGroupSeparator;
            jobj["currency"]["."] = ni.CurrencyDecimalSeparator;
            return jobj;
        }

        private static JObject CreateDateInfoJson(DateTimeFormatInfo di)
        {
            var obj = new
            {
                firstDay = di.FirstDayOfWeek,
                days = new
                {
                    names = di.DayNames,
                    namesAbbr = di.AbbreviatedDayNames,
                    namesShort = di.ShortestDayNames
                },
                months = new
                {
                    names = di.MonthNames,
                    namesAbbr = di.AbbreviatedMonthNames
                },
                AM = new[] { di.AMDesignator, di.AMDesignator.ToLowerInvariant(), di.AMDesignator.ToUpperInvariant() },
                PM = new[] { di.PMDesignator, di.PMDesignator.ToLowerInvariant(), di.PMDesignator.ToUpperInvariant() },
                eras = di.Calendar.Eras.Select(era => new { offset = 0, start = (string)null, name = di.GetEraName(era) }).ToArray(),
                twoDigitYearMax = di.Calendar.TwoDigitYearMax,
                patterns = new
                {
                    d = di.ShortDatePattern,
                    D = di.LongDatePattern,
                    t = di.ShortTimePattern,
                    T = di.LongTimePattern,
                    f = di.LongDatePattern + " " + di.ShortTimePattern,
                    F = di.LongDatePattern + " " + di.LongTimePattern,
                    M = di.MonthDayPattern,
                    Y = di.YearMonthPattern,
                    g = di.ShortDatePattern + " " + di.ShortTimePattern,
                    G = di.ShortDatePattern + " " + di.LongTimePattern
                }
            };
            var jobj = JObject.FromObject(obj);
            if (!di.MonthNames.SequenceEqual(di.MonthGenitiveNames))
            {
                var monthsGenitive = jobj["monthsGenitive"] = new JObject();
                monthsGenitive["names"] = JArray.FromObject(di.MonthGenitiveNames);
                monthsGenitive["namesAbbr"] = JArray.FromObject(di.AbbreviatedMonthGenitiveNames);
            }
            return new JObject()
            {
                {"standard", jobj }
            };
        }
        public static JObject BuildCultureInfoJson(CultureInfo ci)
        {
            var cultureInfoClientObj = new
            {
                name = ci.Name,
                nativeName = ci.NativeName,
                englishName = ci.EnglishName,
                isRTL = ci.TextInfo.IsRightToLeft,
                language = ci.TwoLetterISOLanguageName
            };
            var jobj = JObject.FromObject(cultureInfoClientObj);

            jobj["numberFormat"] = CreateNumberInfoJson(ci.NumberFormat);
            jobj["calendars"] = CreateDateInfoJson(ci.DateTimeFormat);
            return JsonUtils.Diff(defaultJson, jobj);
        }

        public static string BuildCultureInfoScript(CultureInfo ci)
        {
            var template = new JQueryGlobalizeRegisterTemplate();
            template.Name = ci.Name;
            template.CultureInfoJson = BuildCultureInfoJson(ci);

            return template.TransformText();
        }
    }
}
