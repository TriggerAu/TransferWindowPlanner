using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace KSPPluginFramework
{
    public class KSPDateTime : IFormattable
    {
        private CalendarTypeEnum CalType { get { return KSPDateStructure.CalendarType; } }

        
        //Descriptors of DateTime - uses UT as the Root value
        public int Year {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.Year;
            else
                return KSPDateStructure.EpochYear + (Int32)UT / KSPDateStructure.SecondsPerYear; 
            }
        }
        public int DayOfYear {
            get { if (CalType == CalendarTypeEnum.Earth) 
                return _EarthDateTime.DayOfYear;
            else
                return KSPDateStructure.EpochDayOfYear + (Int32)UT / KSPDateStructure.SecondsPerDay % KSPDateStructure.DaysPerYear; 
            }
        }
        public int Day
        {
            get
            {
                if (CalType == CalendarTypeEnum.Earth)
                    return _EarthDateTime.Day;
                else
                    return DayOfMonth;
            }
        }
        public int Month
        {
            get {
                if (CalType == CalendarTypeEnum.Earth)
                    return _EarthDateTime.Month;
                else
                {
                    if (KSPDateStructure.MonthCount < 1)
                        return 0;
                    else
                        return KSPDateStructure.Months.IndexOf(MonthObj)+1;
                }
            }
        }

        private KSPMonth MonthObj {
            get {
                if (KSPDateStructure.MonthCount < 1)
                    return null;
                Int32 monthMaxDay=0;
                for (int i = 0; i < KSPDateStructure.MonthCount; i++){
                    if (DayOfYear <= monthMaxDay + KSPDateStructure.Months[i].Days)
                        return KSPDateStructure.Months[i];
                }
                return KSPDateStructure.Months.Last();
            }
        }
        private Int32 DayOfMonth {
            get {
                if (KSPDateStructure.MonthCount < 1)
                    return DayOfYear;

                Int32 monthMaxDay = 0;
                for (int i = 0; i < KSPDateStructure.MonthCount; i++)
                {
                    if (DayOfYear <= monthMaxDay + KSPDateStructure.Months[i].Days)
                        return DayOfYear - monthMaxDay;
                }
                return DayOfYear;
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
                //if (KSPDateStructure.CalendarType== CalendarTypeEnum.Earth)
                //    return _EarthDateTime.Subtract(KSPDateStructure.CustomEpochEarth).TotalSeconds;
                //else
                    return _TimeSpanFromEpoch.UT; 
            }
            set { 
                _TimeSpanFromEpoch = new KSPTimeSpan(value);
                //if (KSPDateStructure.CalendarType == CalendarTypeEnum.Earth)
                //    _EarthDateTime = KSPDateStructure.CustomEpochEarth.AddSeconds(value);
            } 
        }

        private KSPTimeSpan _TimeSpanFromEpoch;
        private DateTime _EarthDateTime { get { return KSPDateStructure.CustomEpochEarth.AddSeconds(UT); } }
        private DateTime _EarthDateTimeEpoch { get { return new DateTime(KSPDateStructure.EpochYear, 1, KSPDateStructure.EpochDayOfYear); } }

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

            UT = new KSPTimeSpan((year - KSPDateStructure.EpochYear) * KSPDateStructure.DaysPerYear  +
                                (day - KSPDateStructure.EpochDayOfYear),
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
        public KSPTimeSpan TimeOfDay { get { return new KSPTimeSpan(UT % KSPDateStructure.SecondsPerDay); } }


        public static KSPDateTime Now {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()); }
        }
        public static KSPDateTime Today {
            get { return new KSPDateTime(Planetarium.GetUniversalTime()).Date; }
        }
        #endregion


        #region String Formatter

        private AMPMEnum AMPM {
            get {
                if (KSPDateStructure.HoursPerDay % 2 == 0)
                {
                    if (Hour < (KSPDateStructure.HoursPerDay / 2))
                        return AMPMEnum.AM;
                    else
                        return AMPMEnum.PM;
                }
                else
                    return AMPMEnum.OddHoursPerDay;
            }
        }
        private enum AMPMEnum {
            AM,PM,OddHoursPerDay
        }


        public override String ToString()
        {
            if (KSPDateStructure.CalendarType ==CalendarTypeEnum.Earth) {
                return ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
            } else {
                return ToString("Year y, Da\\y d - H\\h, m\\m", null);
            }
        }
        public String ToString(String format)
        {
            return ToString(format, null);
        }
        public String ToString(String format, IFormatProvider provider)
        {
            //parse and replace the format stuff
            MatchCollection matches = Regex.Matches(format, "([a-zA-z])\\1{0,}");
            for (int i = matches.Count-1; i >=0; i--)
            {
                Match m = matches[i];
                Int32 mIndex = m.Index,mLength = m.Length;

                if (mIndex>0 && format[m.Index - 1] == '\\')
                {
                    if (m.Length == 1)
                        continue;
                    else {
                        mIndex++;
                        mLength--;
                    }
                }
                switch (m.Value[0])
                {
                    case 'y': 
                        format = format.Substring(0, mIndex) + Year.ToString("D" + mLength) + format.Substring(mIndex + mLength);
                        break;
                    case 'M':
                        String input2 = Month.ToString("D" + mLength);

                        //test for more than 2 M's
                        format = format.Substring(0, mIndex) + input2 + format.Substring(mIndex + mLength);
                        break;
                    case 'd':
                        format = format.Substring(0, mIndex) + Day.ToString("D" + mLength) + format.Substring(mIndex + mLength);
                        break;
                    case 'h':
                        //how to do this one AM/PM Hours
                        String HalfDayTime="";
                        switch (AMPM)
	                    {
                            case AMPMEnum.AM:
                                HalfDayTime = Hour.ToString("D" + mLength.Clamp(1, (KSPDateStructure.HoursPerDay / 2).ToString().Length));
                                break;
                            case AMPMEnum.PM:
                                HalfDayTime = (Hour - (KSPDateStructure.HoursPerDay / 2)).ToString("D" + mLength.Clamp(1, (KSPDateStructure.HoursPerDay / 2).ToString().Length));
                                break;
                            case AMPMEnum.OddHoursPerDay:
                            default:
                                HalfDayTime = Hour.ToString("D" + mLength.Clamp(1, KSPDateStructure.HoursPerDay.ToString().Length));
                                break;
	                    }

                        format = format.Substring(0, mIndex) + HalfDayTime + format.Substring(mIndex + mLength);
                        break;
                    case 't':
                        if (AMPM != AMPMEnum.OddHoursPerDay)
                            format = format.Substring(0, mIndex) + AMPM.ToString().ToLower() + format.Substring(mIndex + mLength);
                        break;
                    case 'T':
                        if (AMPM != AMPMEnum.OddHoursPerDay)
                            format = format.Substring(0, mIndex) + AMPM.ToString().ToUpper() + format.Substring(mIndex + mLength);
                        break;
                    case 'H':
                        format = format.Substring(0, mIndex) + Hour.ToString("D" + mLength.Clamp(1,KSPDateStructure.HoursPerDay.ToString().Length)) + format.Substring(mIndex + mLength);
                        break;
                    case 'm':
                        format = format.Substring(0, mIndex) + Minute.ToString("D" + mLength.Clamp(1,KSPDateStructure.MinutesPerHour.ToString().Length)) + format.Substring(mIndex + mLength);
                        break;
                    case 's':
                        format = format.Substring(0, mIndex) + Second.ToString("D" + mLength.Clamp(1,KSPDateStructure.SecondsPerMinute.ToString().Length)) + format.Substring(mIndex + mLength);
                        break;

                    default:
                        break;
                }
            }

            //Now strip out the \ , but not multiple \\
            format = Regex.Replace(format, "\\\\(?=[a-z])", "");

            return format;
            //if (KSPDateStructure.CalendarType == CalendarTypeEnum.Earth)
            //    return String.Format(format, _EarthDateTime);
            //else
            //    return String.Format(format, this); //"TEST";
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
            return new KSPDateTime(UT + value * KSPDateStructure.SecondsPerYear);
        }
        public KSPDateTime AddDays(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateStructure.SecondsPerDay);
        }
        public KSPDateTime AddHours(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateStructure.SecondsPerHour);
        }
        public KSPDateTime AddMinutes(Double value)
        {
            return new KSPDateTime(UT + value * KSPDateStructure.SecondsPerMinute);
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

    }
}
