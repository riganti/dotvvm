using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redwood.Framework.Controls;
using Redwood.Framework.Hosting;

namespace Redwood.Samples.BasicSamples.Controls
{
    public class Sample7_Calendar : RedwoodMarkupControl 
    {

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }
        public static RedwoodProperty SelectedDateProperty = RedwoodProperty.RegisterControlStateProperty<DateTime?, Sample7_Calendar>(c => c.SelectedDate);


        public DateTime VisibleDate 
        {
            get { return (DateTime)GetValue(VisibleDateProperty); }
            set { SetValue(VisibleDateProperty, value); }
        }
        public static RedwoodProperty VisibleDateProperty = RedwoodProperty.RegisterControlStateProperty<DateTime, Sample7_Calendar>(c => c.VisibleDate);


        public override bool RequiresControlState
        {
            get { return true; }
        }


        protected override void OnPreRender(RedwoodRequestContext context)
        {
            // ensure the values are in control state           // TODO: this should not be necessary
            if (!context.IsPostBack)
            {
                VisibleDate = DateTime.Now;
            }
            ControlState["VisibleDate"] = VisibleDate;
            ControlState["SelectedDate"] = SelectedDate;
            ControlState["Rows"] = Rows.ToList();
            ControlState["DayNames"] = DayNames.ToArray();
            ControlState["VisibleMonthText"] = VisibleMonthText;
        }



        public string[] DayNames
        {
            get { return CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames; }
        }
        public static RedwoodProperty DayNamesProperty = RedwoodProperty.RegisterControlStateProperty<string[], Sample7_Calendar>(c => c.DayNames);


        public string VisibleMonthText
        {
            get { return VisibleDate.ToString("MMMM yyyy"); }
        }
        public static RedwoodProperty VisibleMonthTextProperty = RedwoodProperty.RegisterControlStateProperty<string, Sample7_Calendar>(c => c.VisibleMonthText);



        public IEnumerable<CalendarRow> Rows
        {
            get
            {
                var firstMonthDate = new DateTime(VisibleDate.Year, VisibleDate.Month, 1);
                var date = firstMonthDate.AddDays(-(int)firstMonthDate.DayOfWeek);

                while (date < firstMonthDate.AddMonths(1))
                {
                    yield return new CalendarRow()
                    {
                        Days = Enumerable.Range(0, 7).Select(i => date.AddDays(i)).Select(d => new CalendarDay()
                        {
                            IsOtherMonth = d.Month != VisibleDate.Month,
                            IsSelected = SelectedDate != null && d == SelectedDate.Value.Date,
                            IsToday = d == DateTime.Today,
                            Number = d.Day,
                            Date = d
                        }).ToArray()
                    };
                    date = date.AddDays(7); 
                }
            }    
        }
        public static RedwoodProperty RowsProperty = RedwoodProperty.RegisterControlStateProperty<IEnumerable<CalendarRow>, Sample7_Calendar>(c => c.Rows);



        public void GoToPreviousMonth()
        {
            VisibleDate = VisibleDate.AddMonths(-1);
        }

        public void GoToNextMonth()
        {
            VisibleDate = VisibleDate.AddMonths(1);
        }

        public void SelectDate(DateTime? date)
        {
            SelectedDate = date;
        }

    }

    public class CalendarRow
    {
        public CalendarDay[] Days { get; set; }
    }

    public class CalendarDay
    {
        public int Number { get; set; }

        public bool IsOtherMonth { get; set; }

        public bool IsSelected { get; set; }

        public bool IsToday { get; set; }
    
        public  DateTime Date { get; set; }
    }
}