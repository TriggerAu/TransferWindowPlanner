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

        static public Int32 CustomEpochDayOfYear { get; private set; }
        static public Int32 CustomEpochYear { get; private set; }

        static public DateTime CustomEpochEarth { get; private set; }

        ////Define the Calendar
        static public Int32 CustomSecondsPerMinute { get; set; }
        static public Int32 CustomMinutesPerHour { get; set; }
        static public Int32 CustomHoursPerDay { get; set; }
        static public Int32 CustomDaysPerYear { get {return CustomDaysPerYear;}
            set { CustomDaysPerYear=value;
            if (CalendarType == CalendarTypeEnum.Custom)
                DaysPerYear = value;
            } 
        }


        static public CalendarTypeEnum CalendarType {get;set;}

        static public void SetCalendarType(CalendarTypeEnum caltype, Int32 EpochYear, Int32 EpochDayOfYear)
        {
            SetCalendarType(caltype);
            CustomEpochYear = EpochYear;
            CustomEpochDayOfYear = EpochDayOfYear;
        }
        static public void SetCalendarType(CalendarTypeEnum caltype)
        {
            CalendarType = caltype;
            switch (caltype)
            {
                case CalendarTypeEnum.KSPStock:
                    EpochYear = 1;
                    EpochDayOfYear = 1;
                    SecondsPerMinute = 60;
                    MinutesPerHour = 60;

                    HoursPerDay = GameSettings.KERBIN_TIME ? 6 : 24;
                    DaysPerYear = GameSettings.KERBIN_TIME ? 425 : 365;
                    break;
                case CalendarTypeEnum.Earth:
                    EpochYear = 1951;
                    EpochDayOfYear = 1;
                    SecondsPerMinute = 60;
                    MinutesPerHour = 60;
                    HoursPerDay = 24;
                    DaysPerYear = 365;
                    break;
                case CalendarTypeEnum.Custom:
                    EpochYear = CustomEpochYear;
                    EpochDayOfYear = CustomEpochDayOfYear;
                    SecondsPerMinute = CustomSecondsPerMinute;
                    MinutesPerHour = CustomMinutesPerHour;
                    HoursPerDay = CustomHoursPerDay;
                    DaysPerYear = CustomDaysPerYear;
                    break;
                default:
                    break;
            }

        }

        static public void SetCalendarTypeEarth(Int32 EpochYear, Int32 EpochMonth, Int32 EpochDay)
        {
            SetCalendarType( CalendarTypeEnum.Earth);

            CustomEpochEarth = new DateTime(EpochYear, EpochMonth, EpochDay);
        }

        //    }
        //}

        static public List<KSPMonth> Months {get; set;}
        static public Int32 MonthCount { get { return Months.Count; } }

        static KSPDateTimeStructure()
        {
            SetCalendarType(CalendarTypeEnum.KSPStock);

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
