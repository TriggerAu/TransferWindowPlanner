using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPDateTime
    {
        private CalendarTypeEnum CalType { get { return KSPDateTimeStructure.CalendarType; } }

        
        //Descriptors of DateTime - uses UT as the Root value
        public int Year {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.Year;
            else
                return KSPDateTimeStructure.EpochYear + (Int32)UT / KSPDateTimeStructure.SecondsPerYear; 
            }
        }
        public int DayOfYear {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.DayOfYear;
            else
                return KSPDateTimeStructure.EpochDayOfYear + (Int32)UT / KSPDateTimeStructure.SecondsPerDay % KSPDateTimeStructure.DaysPerYear; 
            }
        }
        public int Day
        {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.Day;
            else
                return DayOfYear; 
            }
        }
        public int Month
        {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.Month;
            else{
                if (KSPDateTimeStructure.MonthCount<1)
                    return 0; 
                else
                    return 1;
            }
            }
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

        public int Hour { get { if (CalType == CalendarTypeEnum.Earth) return _EarthDateTime.Hour; else return _TimeSpanFromEpoch.Hours; } }
        public int Minute { get { if (CalType == CalendarTypeEnum.Earth) return _EarthDateTime.Minute; else return _TimeSpanFromEpoch.Minutes; } }
        public int Second { get { if (CalType == CalendarTypeEnum.Earth) return _EarthDateTime.Second; else return _TimeSpanFromEpoch.Seconds; } }
        public int Millisecond { get { if (CalType == CalendarTypeEnum.Earth) return _EarthDateTime.Millisecond; else return _TimeSpanFromEpoch.Milliseconds; } }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT {
            get
            {
                if (KSPDateTimeStructure.CalendarType== CalendarTypeEnum.Earth)
                    return _EarthDateTime.Subtract(KSPDateTimeStructure.CustomEpochEarth).TotalSeconds;
                else
                    return _TimeSpanFromEpoch.UT; 
            }
            set { 
                _TimeSpanFromEpoch = new KSPTimeSpan(value);
                if (KSPDateTimeStructure.CalendarType == CalendarTypeEnum.Earth)
                    _EarthDateTime = KSPDateTimeStructure.CustomEpochEarth.AddSeconds(value);
            } 
        }

        private KSPTimeSpan _TimeSpanFromEpoch;
        private DateTime _EarthDateTime;
        private DateTime _EarthDateTimeEpoch { get { return new DateTime(KSPDateTimeStructure.EpochYear, 1, KSPDateTimeStructure.EpochDayOfYear); } }

        #region Constructors
        public KSPDateTime()
        {
            UT = 0;
        }
        public KSPDateTime(int year, int dayofyear)
        {
            UT = new KSPDateTime(year, dayofyear, 0, 0, 0).UT;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second)
        {

            UT = new KSPDateTime(year, day, hour, minute, second, 0).UT;
        }
        public KSPDateTime(int year, int day, int hour, int minute, int second, int millisecond)
        {
            //Test for entering values outside the norm - eg 25 hours, day 600

            UT = new KSPTimeSpan((year - KSPDateTimeStructure.EpochYear) * KSPDateTimeStructure.DaysPerYear  +
                                (day - KSPDateTimeStructure.EpochDayOfYear),
                                hour,
                                minute,
                                second,
                                millisecond
                                ).UT;
        }

        public KSPDateTime(Double ut)
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
            return new KSPDateTime(UT + value * KSPDateTimeStructure.SecondsPerYear);
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
