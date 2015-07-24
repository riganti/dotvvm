using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class Sample7_Calendar : DotvvmMarkupControl 
    {

        public DateTime? SelectedDate
        {
            get { return (DateTime?)GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }
        public static DotvvmProperty SelectedDateProperty = DotvvmProperty.RegisterControlStateProperty<DateTime?, Sample7_Calendar>(c => c.SelectedDate);


        public DateTime VisibleDate 
        {
            get { return (DateTime)GetValue(VisibleDateProperty); }
            set { SetValue(VisibleDateProperty, value); }
        }
        public static DotvvmProperty VisibleDateProperty = DotvvmProperty.RegisterControlStateProperty<DateTime, Sample7_Calendar>(c => c.VisibleDate, DateTime.Now);


        protected override bool RequiresControlState
        {
            get { return true; }
        }
         


        public string[] DayNames
        {
            get { return CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedDayNames; }
        }
        public static DotvvmProperty DayNamesProperty = DotvvmProperty.RegisterControlStateProperty<string[], Sample7_Calendar>(c => c.DayNames);


        public string VisibleMonthText
        {
            get { return VisibleDate.ToString("MMMM yyyy"); }
        }
        public static DotvvmProperty VisibleMonthTextProperty = DotvvmProperty.RegisterControlStateProperty<string, Sample7_Calendar>(c => c.VisibleMonthText);



        public IList<CalendarRow> Rows
        {
            get { return GetRows().ToList(); }  
        }
        public static DotvvmProperty RowsProperty = DotvvmProperty.RegisterControlStateProperty<IEnumerable<CalendarRow>, Sample7_Calendar>(c => c.Rows);

        private IEnumerable<CalendarRow> GetRows()
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