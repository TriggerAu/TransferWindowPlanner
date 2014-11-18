using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public static class KSPDateTimeStructure
    {
        //Define the Epoch
        static public Int32 EpochDayOfYear { get; private set; }
        static public Int32 EpochYear { get; private set; }

        //Define the Calendar
        static public Int32 SecondsPerMinute { get; private set; }
        static public Int32 MinutesPerHour { get; private set; }
        static public Int32 HoursPerDay { get; private set; }
        static public Int32 DaysPerYear { get; private set; }

        static public Int32 SecondsPerHour { get { return SecondsPerMinute * MinutesPerHour; } }
        static public Int32 SecondsPerDay { get { return SecondsPerHour * HoursPerDay; } }
        static public Int32 SecondsPerYear { get { return SecondsPerDay * DaysPerYear; } }

        static public DateTime CustomEpochEarth { get; private set; }

        static public CalendarTypeEnum CalendarType {get; private set;}



        static public void SetKSPStockCalendar()
        {
            CalendarType = CalendarTypeEnum.KSPStock;

            EpochYear = 1;
            EpochDayOfYear = 1;
            SecondsPerMinute = 60;
            MinutesPerHour = 60;

            HoursPerDay = GameSettings.KERBIN_TIME ? 6 : 24;
            DaysPerYear = GameSettings.KERBIN_TIME ? 425 : 365;
        }

        static public void SetEarthCalendar()
        {
            SetEarthCalendar(1951, 1, 1);
        }
        static public void SetEarthCalendar(Int32 epochyear, Int32 epochmonth, Int32 epochday)
        {
            CalendarType = CalendarTypeEnum.Earth;

            CustomEpochEarth = new DateTime(epochyear, epochmonth, epochday);

            EpochYear = epochyear;
            EpochDayOfYear = CustomEpochEarth.DayOfYear;
            SecondsPerMinute = 60;
            MinutesPerHour = 60;

            HoursPerDay = 24;
            DaysPerYear = 365;

        }

        static public void SetCustomCalendar(Int32 CustomEpochYear, Int32 CustomEpochDayOfYear, Int32 CustomDaysPerYear, Int32 CustomHoursPerDay, Int32 CustomMinutesPerHour, Int32 CustomSecondsPerMinute)
        {
            CalendarType = CalendarTypeEnum.Custom;

            EpochYear = CustomEpochYear;
            EpochDayOfYear = CustomEpochDayOfYear;
            SecondsPerMinute = CustomSecondsPerMinute;
            MinutesPerHour = CustomMinutesPerHour;
            HoursPerDay = CustomHoursPerDay;
            DaysPerYear = CustomDaysPerYear;

        }


        static public List<KSPMonth> Months {get; set;}
        static public Int32 MonthCount { get { return Months.Count; } }

        static KSPDateTimeStructure()
        {
            SetKSPStockCalendar();

            Months = new List<KSPMonth>();
        }


        
    }
     

    public enum CalendarTypeEnum
    {
        KSPStock,
        Earth,
        Custom
    }

    public class KSPMonth
    {
        public int Days { get; set; }
        public String Name { get; set; }
    }
}
