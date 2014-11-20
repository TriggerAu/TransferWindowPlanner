using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPDateTime2
    {
        //Descriptors of DateTime - uses UT as the Root value
        public int Year {
            get { return KSPDateTimeStructure.EpochYear + (Int32)UT / KSPDateTimeStructure.SecondsPerYear; }
            set { UT = UT - (Year - KSPDateTimeStructure.EpochYear) * KSPDateTimeStructure.SecondsPerYear + (value - KSPDateTimeStructure.EpochYear) * KSPDateTimeStructure.SecondsPerYear; } 
        }
        public int DayOfYear {
            get { return KSPDateTimeStructure.EpochDayOfYear + (Int32)UT / KSPDateTimeStructure.SecondsPerDay % KSPDateTimeStructure.SecondsPerYear; }
            set { UT = UT - (DayOfYear - KSPDateTimeStructure.EpochDayOfYear) * KSPDateTimeStructure.SecondsPerDay + (value - KSPDateTimeStructure.EpochDayOfYear) * KSPDateTimeStructure.SecondsPerDay; }
        }
        public int Day
        {
            get
            {
                switch (KSPDateTimeStructure.CalendarType)
                {
                    case CalendarTypeEnum.KSPStock: return DayOfYear;
                    case CalendarTypeEnum.Earth:
                        return KSPDateTimeStructure.CustomEpochEarth.AddSeconds(UT).Day;
                    case CalendarTypeEnum.Custom:
                        break;
                    default: return DayOfYear;
                }
                return DayOfYear;
            }
        }
        public int Month
        {
            get
            {
                switch (KSPDateTimeStructure.CalendarType)
                {
                    case CalendarTypeEnum.KSPStock: return 0;
                    case CalendarTypeEnum.Earth:
                        return KSPDateTimeStructure.CustomEpochEarth.AddSeconds(UT).Month;
                    case CalendarTypeEnum.Custom:
                        break;
                    default: return 0;
                }
                return 0;
            }
        }

        public int Hour { get { return _TimeSpan.Hours; } set { _TimeSpan.Hours = value; } }
        public int Minute { get { return _TimeSpan.Minutes; } set { _TimeSpan.Minutes = value; } }
        public int Second { get { return _TimeSpan.Seconds; } set { _TimeSpan.Seconds = value; } }
        public int Millisecond { get { return _TimeSpan.Milliseconds; } set { _TimeSpan.Milliseconds = value; } }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT {
            get
            {
                if (KSPDateTimeStructure.CalendarType== CalendarTypeEnum.Earth)
                    return _EarthDateTime.Subtract(KSPDateTimeStructure.CustomEpochEarth).TotalSeconds;
                else
                    return _TimeSpan.UT; 
            }
            set { 
                _TimeSpan = new KSPTimeSpan2(value);
                if (KSPDateTimeStructure.CalendarType == CalendarTypeEnum.Earth)
                    _EarthDateTime = KSPDateTimeStructure.CustomEpochEarth.AddSeconds(value);
            } 
        }

        private KSPTimeSpan2 _TimeSpan;
        private DateTime _EarthDateTime;
        private DateTime _EarthDateTimeEpoch { get { return new DateTime(KSPDateTimeStructure.EpochYear, 1, KSPDateTimeStructure.EpochDayOfYear); } }

        #region Constructors
        public KSPDateTime2()
        {
            UT = 0;
        }
        public KSPDateTime2(int year, int dayofyear)
            : this()
        {
            Year = year; DayOfYear = dayofyear;
        }
        public KSPDateTime2(int year, int day, int hour, int minute, int second)
            : this(year, day)
        {
            Hour = hour; Minute = minute; Second = second;
        }
        public KSPDateTime2(int year, int day, int hour, int minute, int second, int millisecond)
            : this(year, day, hour, minute, second)
        {
            Millisecond = millisecond;
        }

        public KSPDateTime2(Double ut)
            : this()
        {
            UT = ut;
        } 
        #endregion


        #region Calculated Properties
        public KSPDateTime2 Date { get { return new KSPDateTime2(Year, DayOfYear); } }
        public KSPTimeSpan2 TimeOfDay { get { return new KSPTimeSpan2(UT % KSPDateTimeStructure.SecondsPerDay); } }


        public static KSPDateTime2 Now {
            get { return new KSPDateTime2(Planetarium.GetUniversalTime()); }
        }
        public static KSPDateTime2 Today {
            get { return new KSPDateTime2(Planetarium.GetUniversalTime()).Date; }
        }
        #endregion


        #region Instance Methods
        #region Mathematic Methods
        public KSPDateTime2 Add(KSPTimeSpan2 value)
        {
            return new KSPDateTime2(UT + value.UT);
        }
        public KSPDateTime2 AddYears(Int32 value)
        {
            KSPDateTime2 dteReturn = new KSPDateTime2(UT);
            dteReturn.Year+=value;
            return dteReturn;
        }
        public KSPDateTime2 AddDays(Double value)
        {
            return new KSPDateTime2(UT + value * KSPDateTimeStructure.SecondsPerDay);
        }
        public KSPDateTime2 AddHours(Double value)
        {
            return new KSPDateTime2(UT + value * KSPDateTimeStructure.SecondsPerHour);
        }
        public KSPDateTime2 AddMinutes(Double value)
        {
            return new KSPDateTime2(UT + value * KSPDateTimeStructure.SecondsPerMinute);
        }
        public KSPDateTime2 AddSeconds(Double value)
        {
            return new KSPDateTime2(UT + value);
        }
        public KSPDateTime2 AddMilliSeconds(Double value)
        {
            return new KSPDateTime2(UT + value / 1000);
        }
        public KSPDateTime2 AddUT(Double value)
        {
            return new KSPDateTime2(UT + value);
        }

        public KSPDateTime2 Subtract(KSPDateTime2 value)
        {
            return new KSPDateTime2(UT - value.UT);
        }
        public KSPTimeSpan2 Subtract(KSPTimeSpan2 value)
        {
            return new KSPTimeSpan2(UT - value.UT);
        }

        #endregion


        #region Comparison Methods
        public Int32 CompareTo(KSPDateTime2 value) {
            return KSPDateTime2.Compare(this, value);
        }
        public Int32 CompareTo(System.Object value) {
            return this.CompareTo((KSPDateTime2)value);
        }
        public Boolean Equals(KSPDateTime2 value) {
            return KSPDateTime2.Equals(this, value);
        }
        public override bool Equals(System.Object obj) {
            return this.Equals((KSPDateTime2)obj);
        }
        #endregion        
        

        public override int GetHashCode() {
            return UT.GetHashCode();
        }

        #endregion


        #region Static Methods
        public static Int32 Compare(KSPDateTime2 t1, KSPDateTime2 t2)
        {
            if (t1.UT < t2.UT)
                return -1;
            else if (t1.UT > t2.UT)
                return 1;
            else
                return 0;
        }
        public static Boolean Equals(KSPDateTime2 t1, KSPDateTime2 t2)
        {
            return t1.UT == t2.UT;
        }


        #endregion

        #region Operators
        public static KSPTimeSpan2 operator -(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return new KSPTimeSpan2(d1.UT - d2.UT);
        }
        public static KSPDateTime2 operator -(KSPDateTime2 d, KSPTimeSpan2 t)
        {
            return new KSPDateTime2(d.UT - t.UT);
        }
        public static KSPDateTime2 operator +(KSPDateTime2 d, KSPTimeSpan2 t)
        {
            return new KSPDateTime2(d.UT + t.UT);
        }
        public static Boolean operator !=(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return !(d1 == d2);
        }
        public static Boolean operator ==(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return d1.UT == d2.UT;
        }



        public static Boolean operator <=(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return d1.CompareTo(d2) <= 0;
        }
        public static Boolean operator <(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return d1.CompareTo(d2) < 0;
        }
        public static Boolean operator >=(KSPDateTime2 d1, KSPDateTime2 d2)
        {
            return d1.CompareTo(d2) >= 0;
        }
        public static Boolean operator >(KSPDateTime2 d1, KSPDateTime2 d2)
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
