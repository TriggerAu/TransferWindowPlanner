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
        public virtual int Year
        {
            get { return (Int32)UT / SecondsPerYear; }
            set { UT = UT - Year * SecondsPerYear + value * SecondsPerYear; }
        }
        public virtual int Day
        {
            get { return (Int32)UT / SecondsPerDay % SecondsPerYear; }
            set { UT = UT - Day * SecondsPerDay + value * SecondsPerDay; }
        }
        public int Hour
        {
            get { return (Int32)UT / SecondsPerHour % SecondsPerDay; }
            set { UT = UT - Hour * SecondsPerHour + value * SecondsPerHour; }
        }
        public int Minute
        {
            get { return (Int32)UT / SecondsPerMinute % SecondsPerHour; }
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


    public class KSPDateTime2 : KSPDateTimeSpanAbstract
    {
        //Define the Epoch
        static public int EpochDay { get; set; }
        static public int EpochYear { get; set; }

        static KSPDateTime2()
        {
            EpochYear = 1;
            EpochDay = 1;

        }

        public override int Year {
            get {
                return base.Year + EpochYear;
            }
            set {
                base.Year = value - EpochYear;
            }
        }
        public override int Day {
            get {
                return base.Day + EpochDay;
            }
            set {
                base.Day = value + EpochDay;
            }
        }
    }
    public class KSPTimeSpan2 : KSPDateTimeSpanAbstract
    {

    }

}
