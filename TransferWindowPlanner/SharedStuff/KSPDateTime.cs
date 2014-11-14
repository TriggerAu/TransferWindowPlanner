using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public struct KSPDateTime
    {
        //Define the Epoch
        static public int EpochDay { get; set; }
        static public int EpochYear { get; set; }

        //Define the Calendar
        static public int SecondsPerMinute { get; set; }
        static public int MinutesPerHour { get; set; }
        static public int HoursPerDay { get; set; }
        static public int DaysPerYear { get; set; }

        static public int SecondsPerHour { get { return SecondsPerMinute * MinutesPerHour; } }
        static public int SecondsPerDay { get { return SecondsPerHour * HoursPerDay; } }
        static public int SecondsPerYear { get { return SecondsPerDay * DaysPerYear; } }


        //Descriptors of DateTime
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Millisecond { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Year { get; set; }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT { get; set; }


        #region Constructors
        static public KSPDateTime()
        {
            EpochYear = 1;
            EpochDay = 1;
            SecondsPerMinute = 60;
            MinutesPerHour = 60;
            HoursPerDay = 6;
            DaysPerYear = 425;
        }


        public KSPDateTime()
        {

        }
        public KSPDateTime(int year, int day)
            : this()
        {
            Year = year; Day = day;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second)
            : this(year, day)
        {
            Hour = hour; Minute = minute; Second = second;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second, int millisecond)
            : this(year, day, hour, minute, second)
        {
            Millisecond = millisecond;
        }

        public KSPDateTime(Double ut)
            : this()
        {

        } 
        #endregion





        public KSPDateTime Date { get { return new KSPDateTime(Year,Day); } }
        public KSPTimeSpan TimeOfDay { get { return new KSPTimeSpan(UT % SecondsPerDay); } }

    }


    public struct MyStruct
    {
        
    }

    public class KSPMonth
    {
        public int Days { get; set; }
        public String Name { get; set; }
    }
}
