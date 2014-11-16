using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPDateTime1
    {
       
        //Descriptors of DateTime - uses UT as the Root value
        public int Year {
            get { return KSPDateTimeStructure.EpochYear + (Int32)UT / KSPDateTimeStructure.SecondsPerYear; }
            set { UT = UT - (Year - KSPDateTimeStructure.EpochYear) * KSPDateTimeStructure.SecondsPerYear + (value - KSPDateTimeStructure.EpochYear) * KSPDateTimeStructure.SecondsPerYear; } 
        }
        public int Day {
            get { return KSPDateTimeStructure.EpochDay + (Int32)UT / KSPDateTimeStructure.SecondsPerDay % KSPDateTimeStructure.SecondsPerYear; }
            set { UT = UT - (Day - KSPDateTimeStructure.EpochDay) * KSPDateTimeStructure.SecondsPerDay + (value - KSPDateTimeStructure.EpochDay) * KSPDateTimeStructure.SecondsPerDay; } 
        }
        public int Hour {
            get { return (Int32)UT / KSPDateTimeStructure.SecondsPerHour % KSPDateTimeStructure.SecondsPerDay; }
            set { UT = UT - Hour * KSPDateTimeStructure.SecondsPerHour + value * KSPDateTimeStructure.SecondsPerHour; } 
        }
        public int Minute {
            get { return (Int32)UT / KSPDateTimeStructure.SecondsPerMinute % KSPDateTimeStructure.SecondsPerHour; }
            set { UT = UT - Minute * KSPDateTimeStructure.SecondsPerMinute + value * KSPDateTimeStructure.SecondsPerMinute; } 
        }
        public int Second {
            get { return (Int32)UT % KSPDateTimeStructure.SecondsPerMinute; } 
            set { UT = UT - Second + value; } 
        }
        public int Millisecond { 
            get { return (Int32) (Math.Round(UT - Math.Floor(UT), 3) * 1000); } 
            set { UT = Math.Floor(UT) + ((Double)value / 1000); } 
        }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT { get { return _UT; } set { _UT = value; } }

        private Double _UT;


        #region Constructors
        public KSPDateTime1()
        {
            _UT = 0;

        }
        public KSPDateTime1(int year, int day)
            : this()
        {
            Year = year; Day = day;
        }
        public KSPDateTime1(int year, int day, int hour, int minute, int second)
            : this(year, day)
        {
            Hour = hour; Minute = minute; Second = second;
        }
        public KSPDateTime1(int year, int day, int hour, int minute, int second, int millisecond)
            : this(year, day, hour, minute, second)
        {
            Millisecond = millisecond;
        }

        public KSPDateTime1(Double ut)
            : this()
        {
            UT = ut;
        } 
        #endregion


        #region Calculated Properties
        public KSPDateTime1 Date { get { return new KSPDateTime1(Year, Day); } }
        public KSPTimeSpan TimeOfDay { get { return new KSPTimeSpan(UT % KSPDateTimeStructure.SecondsPerDay); } }


        public static KSPDateTime1 Now {
            get { return new KSPDateTime1(Planetarium.GetUniversalTime()); }
        }
        public static KSPDateTime1 Today {
            get { return new KSPDateTime1(Planetarium.GetUniversalTime()).Date; }
        }
        #endregion


        #region Instance Methods
        #region Mathematic Methods
        public KSPDateTime1 Add(KSPTimeSpan value)
        {
            return new KSPDateTime1(UT + value.UT);
        }
        public KSPDateTime1 AddYears(Double value)
        {
            return new KSPDateTime1(UT + value * KSPDateTimeStructure.SecondsPerYear);
        }
        public KSPDateTime1 AddDays(Double value)
        {
            return new KSPDateTime1(UT + value * KSPDateTimeStructure.SecondsPerDay);
        }
        public KSPDateTime1 AddHours(Double value)
        {
            return new KSPDateTime1(UT + value * KSPDateTimeStructure.SecondsPerHour);
        }
        public KSPDateTime1 AddMinutes(Double value)
        {
            return new KSPDateTime1(UT + value * KSPDateTimeStructure.SecondsPerMinute);
        }
        public KSPDateTime1 AddSeconds(Double value)
        {
            return new KSPDateTime1(UT + value);
        }
        public KSPDateTime1 AddMilliSeconds(Double value)
        {
            return new KSPDateTime1(UT + value / 1000);
        }
        public KSPDateTime1 AddUT(Double value)
        {
            return new KSPDateTime1(UT + value);
        }

        public KSPDateTime1 Subtract(KSPDateTime1 value)
        {
            return new KSPDateTime1(UT - value.UT);
        }
        public KSPTimeSpan Subtract(KSPTimeSpan value)
        {
            return new KSPTimeSpan(UT - value.UT);
        }

        #endregion


        #region Comparison Methods
        public Int32 CompareTo(KSPDateTime1 value) {
            return KSPDateTime1.Compare(this, value);
        }
        public Int32 CompareTo(System.Object value) {
            return this.CompareTo((KSPDateTime1)value);
        }
        public Boolean Equals(KSPDateTime1 value) {
            return KSPDateTime1.Equals(this, value);
        }
        public override bool Equals(System.Object obj) {
            return this.Equals((KSPDateTime1)obj);
        }
        #endregion        
        

        public override int GetHashCode() {
            return UT.GetHashCode();
        }

        #endregion


        #region Static Methods
        public static Int32 Compare(KSPDateTime1 t1, KSPDateTime1 t2)
        {
            if (t1.UT < t2.UT)
                return -1;
            else if (t1.UT > t2.UT)
                return 1;
            else
                return 0;
        }
        public static Boolean Equals(KSPDateTime1 t1, KSPDateTime1 t2)
        {
            return t1.UT == t2.UT;
        }


        #endregion

        #region Operators
        public static KSPTimeSpan operator -(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return new KSPTimeSpan(d1.UT - d2.UT);
        }
        public static KSPDateTime1 operator -(KSPDateTime1 d, KSPTimeSpan t)
        {
            return new KSPDateTime1(d.UT - t.UT);
        }
        public static KSPDateTime1 operator +(KSPDateTime1 d, KSPTimeSpan t)
        {
            return new KSPDateTime1(d.UT + t.UT);
        }
        public static Boolean operator !=(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return !(d1 == d2);
        }
        public static Boolean operator ==(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return d1.UT == d2.UT;
        }



        public static Boolean operator <=(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return d1.CompareTo(d2) <= 0;
        }
        public static Boolean operator <(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return d1.CompareTo(d2) < 0;
        }
        public static Boolean operator >=(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return d1.CompareTo(d2) >= 0;
        }
        public static Boolean operator >(KSPDateTime1 d1, KSPDateTime1 d2)
        {
            return d1.CompareTo(d2) > 0;
        } 
        #endregion

        //DaysInMonth
        //Day - is Day Of Month
        //DayOfYear
        //IsLeapYear


        //To String Formats
    }




}
