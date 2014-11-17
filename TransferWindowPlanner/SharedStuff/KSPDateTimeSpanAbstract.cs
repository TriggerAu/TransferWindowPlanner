using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransferWindowPlanner
{
    public abstract class KSPDateTimeSpanAbstract
    {
        //Define the Calendar
        static public int SecondsPerMinute { get; set; }
        static public int MinutesPerHour { get; set; }
        static public int HoursPerDay { get; set; }
        static public int DaysPerYear { get; set; }

        static public int SecondsPerHour { get { return SecondsPerMinute * MinutesPerHour; } }
        static public int SecondsPerDay { get { return SecondsPerHour * HoursPerDay; } }
        static public int SecondsPerYear { get { return SecondsPerDay * DaysPerYear; } }

        static KSPDateTimeSpanAbstract()
        {
            SecondsPerMinute = 60;
            MinutesPerHour = 60;
            HoursPerDay = 6;
            DaysPerYear = 425;
        }

        //Descriptors of DateTime - uses UT as the Root value
        private int _Year;
        protected virtual int Year
        {
            get { return (Int32)UT / SecondsPerYear; }
            set { 
                UT = UT - _Year * SecondsPerYear + value * SecondsPerYear;
                _Year = value;
            }
        }

        private int _Day;
        protected virtual int Day
        {
            get { return (Int32)UT / SecondsPerDay % DaysPerYear; }
            set { 
                UT = UT - _Day * SecondsPerDay + value * SecondsPerDay;
                _Day = value;
            }
        }
        public int Hour
        {
            get { return (Int32)UT / SecondsPerHour % HoursPerDay; }
            set { UT = UT - Hour * SecondsPerHour + value * SecondsPerHour; }
        }
        public int Minute
        {
            get { return (Int32)UT / SecondsPerMinute % MinutesPerHour; }
            set { UT = UT - Minute * SecondsPerMinute + value * SecondsPerMinute; }
        }
        public int Second
        {
            get { return (Int32)UT % SecondsPerMinute; }
            set { UT = UT - Second + value; }
        }
        public int Millisecond
        {
            get { return (Int32)(Math.Round(UT - Math.Floor(UT), 3) * 1000); }
            set { UT = Math.Floor(UT) + ((Double)value / 1000); }
        }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT { get { return _UT; } set { _UT = value; } }

        private Double _UT;
    }


    public class KSPDateTime20 : KSPDateTimeSpanAbstract
    {
        //Define the Epoch
        static public int EpochDay { get; set; }
        static public int EpochYear { get; set; }

        static KSPDateTime20()
        {
            EpochYear = 1;
            EpochDay = 1;

        }

        new public int Year {
            get { return base.Year + EpochYear; }
            set { base.Year = value - EpochYear; }
        }
        new public int Day {
            get { return base.Day + EpochDay; }
            set { base.Day = value - EpochDay; }
        }
        new public int Hour { get { return base.Hour; } set { base.Hour = value; } }
        new public int Minute { get { return base.Minute; } set { base.Minute = value; } }
        new public int Second { get { return base.Second; } set { base.Second = value; } }
        new public int Millisecond { get { return base.Millisecond; } set { base.Millisecond = value; } }

    }
    public class KSPTimeSpan20 : KSPDateTimeSpanAbstract
    {
        public int Years { get { return base.Year; } set { base.Year = value; } }
        public int Days { get { return base.Day; } set { base.Day = value; } }
        public int Hours { get { return base.Hour; } set { base.Hour = value; } }
        public int Minutes { get { return base.Minute; } set { base.Minute = value; } }
        public int Seconds { get { return base.Second; } set { base.Second = value; } }
        public int Milliseconds { get { return base.Millisecond; } set { base.Millisecond = value; } }


    }

}
