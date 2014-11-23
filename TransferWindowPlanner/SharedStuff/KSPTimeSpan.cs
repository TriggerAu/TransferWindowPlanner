using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace KSPPluginFramework
{
    public class KSPTimeSpan : IFormattable
    {
        //Descriptors of Timespan - uses UT as the Root value
        public Int32 Days {
            get { return (Int32)UT / KSPDateStructure.SecondsPerDay; }
        }
        public int Hours {
            get { return (Int32)UT / KSPDateStructure.SecondsPerHour % KSPDateStructure.HoursPerDay; }
        }
        public int Minutes {
            get { return (Int32)UT / KSPDateStructure.SecondsPerMinute % KSPDateStructure.MinutesPerHour; }
        }
        public int Seconds {
            get { return (Int32)UT % KSPDateStructure.SecondsPerMinute; }
        }
        public int Milliseconds {
            get { return (Int32)(Math.Round(UT - Math.Floor(UT), 3) * 1000); }
        }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT
        /// </summary>
        public Double UT { get; set; }

        #region Constructors
        public KSPTimeSpan()
        {
            UT = 0;
        }
        public KSPTimeSpan(int hours, int minutes, int seconds)
        {
            UT = new KSPTimeSpan(0, hours, minutes, seconds, 0).UT;
        }
        public KSPTimeSpan(int days, int hours, int minutes, int seconds)
        {
            UT = new KSPTimeSpan(days, hours, minutes, seconds, 0).UT;
        }
        public KSPTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            UT = days * KSPDateStructure.SecondsPerDay +
                 hours * KSPDateStructure.SecondsPerHour +
                 minutes * KSPDateStructure.SecondsPerMinute +
                 seconds +
                (Double)milliseconds / 1000;
        }

        public KSPTimeSpan(Double ut)
            : this()
        {
            UT = ut;
        } 
        #endregion


        #region Calculated Properties
        public Double TotalMilliseconds { get { return UT * 1000; } }
        public Double TotalSeconds { get { return UT; } }
        public Double TotalMinutes { get { return UT / KSPDateStructure.SecondsPerMinute; } }
        public Double TotalHours { get { return UT / KSPDateStructure.SecondsPerHour; } }
        public Double TotalDays { get { return UT / KSPDateStructure.SecondsPerDay; } }
        #endregion

        #region String Formatter

        public override String ToString()
        {
            return ToString(3);
        }

        public String ToString(Int32 Precision)
        {
            Int32 Displayed = 0;
            String format = "";

            if(Precision > 3 || Days>0 ){
                format = "d\\d,";
                Displayed++;
            }
            if ((Hours>0 && Displayed<Precision)) {
                format += (format==""?"":" ") + "h\\h,";
                Displayed++;

            }
            if ((Minutes>0 && Displayed<Precision)) {
                format += (format==""?"":" ") + "m\\m,";
                Displayed++;

            }
            if ((Seconds>0 && Displayed<Precision)) {
                format += (format==""?"":" ") + "s\\s,";
                Displayed++;

            }

            format = format.TrimEnd(',');

            return ToString(format, null);
        }

        public String ToString(String format)
        {
            return ToString(format, null);
        }
        public String ToString(String format, IFormatProvider provider)
        {
            //parse and replace the format stuff
            MatchCollection matches = Regex.Matches(format, "([a-zA-z])\\1{0,}");
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                Match m = matches[i];
                Int32 mIndex = m.Index, mLength = m.Length;

                if (mIndex > 0 && format[m.Index - 1] == '\\')
                {
                    if (m.Length == 1)
                        continue;
                    else
                    {
                        mIndex++;
                        mLength--;
                    }
                }
                switch (m.Value[0])
                {
                    case 'd':
                        format = format.Substring(0, mIndex) + Days.ToString("D" + mLength) + format.Substring(mIndex + mLength);
                        break;
                    case 'h':
                        format = format.Substring(0, mIndex) + Hours.ToString("D" + mLength.Clamp(1, KSPDateStructure.HoursPerDay.ToString().Length)) + format.Substring(mIndex + mLength);
                        break;
                    case 'm':
                        format = format.Substring(0, mIndex) + Minutes.ToString("D" + mLength.Clamp(1, KSPDateStructure.MinutesPerHour.ToString().Length)) + format.Substring(mIndex + mLength);
                        break;
                    case 's':
                        format = format.Substring(0, mIndex) + Seconds.ToString("D" + mLength.Clamp(1, KSPDateStructure.SecondsPerMinute.ToString().Length)) + format.Substring(mIndex + mLength);
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
        public KSPTimeSpan Add(KSPTimeSpan value) {
            return new KSPTimeSpan(UT + value.UT);
        }
        public KSPTimeSpan Duration() {
            return new KSPTimeSpan(Math.Abs(UT));
        }
        public KSPTimeSpan Negate() {
            return new KSPTimeSpan(UT*-1);
        }
        #endregion

        #region Comparison Methods
        public Int32 CompareTo(KSPTimeSpan value) {
            return KSPTimeSpan.Compare(this, value);
        }
        public Int32 CompareTo(System.Object value) {
            return this.CompareTo((KSPTimeSpan)value);
        }
        public Boolean Equals(KSPTimeSpan value) {
            return KSPTimeSpan.Equals(this, value);
        }
        public override bool Equals(System.Object obj) {
            return this.Equals((KSPTimeSpan)obj);
        }
        #endregion        
        

        public override int GetHashCode()
        {
            return UT.GetHashCode();
        }

        #endregion


        #region Static Methods
        public static Int32 Compare(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            if (t1.UT < t2.UT)
                return -1;
            else if (t1.UT > t2.UT)
                return 1;
            else
                return 0;
        }
        public static Boolean Equals(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.UT == t2.UT;
        }


        public static KSPTimeSpan FromDays(Double value) {
            return new KSPTimeSpan(value * KSPDateStructure.SecondsPerDay);
        }
        public static KSPTimeSpan FromHours(Double value) {
            return new KSPTimeSpan(value * KSPDateStructure.SecondsPerHour);
        }
        public static KSPTimeSpan FromMinutes(Double value) {
            return new KSPTimeSpan(value * KSPDateStructure.SecondsPerMinute);
        }
        public static KSPTimeSpan FromSeconds(Double value) {
            return new KSPTimeSpan(value);
        }
        public static KSPTimeSpan FromMilliseconds(Double value) {
            return new KSPTimeSpan(value / 1000);
        }

        #endregion

        #region Operators
        public static KSPTimeSpan operator -(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return new KSPTimeSpan(t1.UT - t2.UT);
        }
        public static KSPTimeSpan operator -(KSPTimeSpan t)
        {
            return new KSPTimeSpan(t.UT).Negate();
        }
        public static KSPTimeSpan operator +(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return new KSPTimeSpan(t1.UT + t2.UT);
        }
        public static KSPTimeSpan operator +(KSPTimeSpan t)
        {
            return new KSPTimeSpan(t.UT);
        }

        public static Boolean operator !=(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return !(t1 == t2);
        }
        public static Boolean operator ==(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.UT == t2.UT;
        }



        public static Boolean operator <=(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.CompareTo(t2) <= 0;
        }
        public static Boolean operator <(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.CompareTo(t2) < 0;
        }
        public static Boolean operator >=(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.CompareTo(t2) >= 0;
        }
        public static Boolean operator >(KSPTimeSpan t1, KSPTimeSpan t2)
        {
            return t1.CompareTo(t2) > 0;
        }
        #endregion



        //To String Formats
    }
}
