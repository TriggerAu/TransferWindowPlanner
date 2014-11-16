using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPDateTime
    {
        private Boolean EarthTime = false;

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
                        break;
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
                        return new DateTime()
                        break;
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
                    return _EarthDateTime.Subtract(new DateTime(KSPDateTimeStructure.EpochYear,1,KSPDateTimeStructure.EpochDayOfYear)).TotalSeconds;
                else
                    return _TimeSpan.UT; 
            }
            set { 
                _TimeSpan = new KSPTimeSpan(value);
                if (KSPDateTimeStructure.CalendarType == CalendarTypeEnum.Earth) 
                    _EarthDateTime = new DateTime(KSPDateTimeStructure.EpochYear, 1, KSPDateTimeStructure.EpochDayOfYear).AddSeconds(value);
            } 
        }

        private KSPTimeSpan _TimeSpan;
        private DateTime _EarthDateTime;
        private DateTime _EarthDateTimeEpoch { get { return new DateTime(KSPDateTimeStructure.EpochYear, 1, KSPDateTimeStructure.EpochDayOfYear); } }

        #region Constructors
        public KSPDateTime()
        {
            UT = 0;
        }
        public KSPDateTime(int year, int dayofyear)
            : this()
        {
            Year = year; DayOfYear = dayofyear;
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
            UT = ut;
        } 
        #endregion


        #region Calculated Properties
        public KSPDateTime Date { get { return new KSPDateTime(Year, DayOfYear); } }
        public KSPTimeSpan TimeOfDay { get { return new KSPTimeSpan(UT % KSPDateTimeStructure.SecondsPerDay); } }


        public static KSPDateTime Now {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()); }
        }
        public static KSPDateTime Today {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()).Date; }
        }
        #endregion


        #region Instance Methods
        #region Mathematic Methods
        public KSPDateTime Add(KSPTimeSpan value)
        {
            return new KSPDateTime(UT + value.UT);
        }
        public KSPDateTime AddYears(Int32 value)
        {
            KSPDateTime dteReturn = new KSPDateTime(UT);
            dteReturn.Year+=value;
            return dteReturn;
        }
        public KSPDateTime AddDays(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateTimeStructure.SecondsPerDay);
        }
        public KSPDateTime AddHours(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateTimeStructure.SecondsPerHour);
        }
        public KSPDateTime AddMinutes(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateTimeStructure.SecondsPerMinute);
        }
        public KSPDateTime AddSeconds(Double value)
        {
            return new KSPDateTime(UT + value);
        }
        public KSPDateTime AddMilliSeconds(Double value)
        {
            return new KSPDateTime(UT + value / 1000);
        }
        public KSPDateTime AddUT(Double value)
        {
            return new KSPDateTime(UT + value);
        }

        public KSPDateTime Subtract(KSPDateTime value)
        {
            return new KSPDateTime(UT - value.UT);
        }
        public KSPTimeSpan Subtract(KSPTimeSpan value)
        {
            return new KSPTimeSpan(UT - value.UT);
        }

        #endregion


        #region Comparison Methods
        public Int32 CompareTo(KSPDateTime value) {
            return KSPDateTime.Compare(this, value);
        }
        public Int32 CompareTo(System.Object value) {
            return this.CompareTo((KSPDateTime)value);
        }
        public Boolean Equals(KSPDateTime value) {
            return KSPDateTime.Equals(this, value);
        }
        public override bool Equals(System.Object obj) {
            return this.Equals((KSPDateTime)obj);
        }
        #endregion        
        

        public override int GetHashCode() {
            return UT.GetHashCode();
        }

        #endregion


        #region Static Methods
        public static Int32 Compare(KSPDateTime t1, KSPDateTime t2)
        {
            if (t1.UT < t2.UT)
                return -1;
            else if (t1.UT > t2.UT)
                return 1;
            else
                return 0;
        }
        public static Boolean Equals(KSPDateTime t1, KSPDateTime t2)
        {
            return t1.UT == t2.UT;
        }


        #endregion

        #region Operators
        public static KSPTimeSpan operator -(KSPDateTime d1, KSPDateTime d2)
        {
            return new KSPTimeSpan(d1.UT - d2.UT);
        }
        public static KSPDateTime operator -(KSPDateTime d, KSPTimeSpan t)
        {
            return new KSPDateTime(d.UT - t.UT);
        }
        public static KSPDateTime operator +(KSPDateTime d, KSPTimeSpan t)
        {
            return new KSPDateTime(d.UT + t.UT);
        }
        public static Boolean operator !=(KSPDateTime d1, KSPDateTime d2)
        {
            return !(d1 == d2);
        }
        public static Boolean operator ==(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.UT == d2.UT;
        }



        public static Boolean operator <=(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) <= 0;
        }
        public static Boolean operator <(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) < 0;
        }
        public static Boolean operator >=(KSPDateTime d1, KSPDateTime d2)
        {
            return d1.CompareTo(d2) >= 0;
        }
        public static Boolean operator >(KSPDateTime d1, KSPDateTime d2)
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
