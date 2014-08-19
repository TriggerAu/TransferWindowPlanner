using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSP;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    /// <summary>
    /// A class to store the UT of events and get back useful data
    /// </summary>
    public class KSPTime
    {

        //really there are 31,446,925.9936 seconds in a year, use 365*24 so the reciprocal math 
        //  to go to years and get back to full days isnt confusing - have to sort this out some day though.
        //NOTE: KSP Dates appear to all be 365 * 24 as well - no fractions - woohoo
        const double HoursPerDayEarth = 24;
        const double HoursPerYearEarth = 365 * HoursPerDayEarth;

        const double HoursPerDayKerbin = 6;
        const double HoursPerYearKerbin = 426 * HoursPerDayKerbin;

        #region "Constructors"
        public KSPTime()
        { }
        public KSPTime(double NewUT)
        {
            UT = NewUT;
        }
        public KSPTime(double Years, double Days, double Hours, double Minutes, double Seconds)
        {
            UT = KSPTime.BuildUTFromRaw(Years, Days, Hours, Minutes, Seconds);
        }
        #endregion

        /// <summary>
        /// Build the UT from raw values
        /// </summary>
        /// <param name="Years"></param>
        /// <param name="Days"></param>
        /// <param name="Hours"></param>
        /// <param name="Minutes"></param>
        /// <param name="Seconds"></param>
        public void BuildUT(double Years, double Days, double Hours, double Minutes, double Seconds)
        {
            UT = KSPTime.BuildUTFromRaw(Years, Days, Hours, Minutes, Seconds);
        }

        public void BuildUT(String Years, String Days, String Hours, String Minutes, String Seconds)
        {
            BuildUT(Convert.ToDouble(Years), Convert.ToDouble(Days), Convert.ToDouble(Hours), Convert.ToDouble(Minutes), Convert.ToDouble(Seconds));
        }

        #region "Properties"

        //Stores the Universal Time in game seconds
        public double UT;

        //readonly props that resolve from UT
        public long Second
        {
            get { return Convert.ToInt64(Math.Truncate(UT % 60)); }
        }
        public long Minute
        {
            get { return Convert.ToInt64(Math.Truncate((UT / 60) % 60)); }
        }

        private double HourRaw { get { return UT / 60 / 60; } }

        public long Hour
        {
            get
            {
                if (GameSettings.KERBIN_TIME) {
                    return Convert.ToInt64(Math.Truncate(HourRaw % HoursPerDayKerbin));
                }
                else {
                    return Convert.ToInt64(Math.Truncate(HourRaw % HoursPerDayEarth));
                }
            }
        }

        public long Day
        {
            get
            {
                if (GameSettings.KERBIN_TIME) {
                    return Convert.ToInt64(Math.Truncate(((HourRaw % HoursPerYearKerbin) / HoursPerDayKerbin)));
                }
                else {
                    return Convert.ToInt64(Math.Truncate(((HourRaw % HoursPerYearEarth) / HoursPerDayEarth)));
                }
            }
        }

        public long Year
        {
            get
            {
                if (GameSettings.KERBIN_TIME) {
                    return Convert.ToInt64(Math.Truncate((HourRaw / HoursPerYearKerbin)));
                }
                else {
                    return Convert.ToInt64(Math.Truncate((HourRaw / HoursPerYearEarth)));
                }
            }
        }
        //public long HourKerbin
        //{
        //    get { return Convert.ToInt64(Math.Truncate(HourRaw % HoursPerDayKerbin)); }
        //}

        //public long DayKerbin
        //{
        //    get { return Convert.ToInt64(Math.Truncate(((HourRaw % HoursPerYearKerbin) / HoursPerDayKerbin))); }
        //}

        //public long YearKerbin
        //{
        //    get { return Convert.ToInt64(Math.Truncate((HourRaw / HoursPerYearKerbin))); }
        //}        
        #endregion

        #region "String Formatting"
        public String IntervalString()
        {
            return IntervalString(6);
        }
        public String IntervalString(int segments)
        {
            String strReturn = "";

            if (UT < 0) strReturn += "+ ";

            int intUsed = 0;

            if (intUsed < segments && Year != 0) {
                strReturn += String.Format("{0}y", Math.Abs(Year));
                intUsed++;
            }

            if (intUsed < segments && (Day != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0}d", Math.Abs(Day));
                intUsed++;
            }

            if (intUsed < segments && (Hour != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0}h", Math.Abs(Hour));
                intUsed++;
            }
            if (intUsed < segments && (Minute != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0}m", Math.Abs(Minute));
                intUsed++;
            }
            if (intUsed < segments)// && (Second != 0 || intUsed > 0))
            {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0}s", Math.Abs(Second));
                intUsed++;
            }


            return strReturn;
        }

        public String IntervalDateTimeString()
        {
            return IntervalDateTimeString(6);
        }
        public String IntervalDateTimeString(int segments)
        {
            String strReturn = "";

            if (UT < 0) strReturn += "+ ";

            int intUsed = 0;

            if (intUsed < segments && Year != 0) {
                strReturn += String.Format("{0}y", Math.Abs(Year));
                intUsed++;
            }

            if (intUsed < segments && (Day != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0}d", Math.Abs(Day));
                intUsed++;
            }

            if (intUsed < segments && (Hour != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ", ";
                strReturn += String.Format("{0:00}", Math.Abs(Hour));
                intUsed++;
            }
            if (intUsed < segments && (Minute != 0 || intUsed > 0)) {
                if (intUsed > 0) strReturn += ":";
                strReturn += String.Format("{0:00}", Math.Abs(Minute));
                intUsed++;
            }
            if (intUsed < segments)// && (Second != 0 || intUsed > 0))
            {
                if (intUsed > 0) strReturn += ":";
                strReturn += String.Format("{0:00}", Math.Abs(Second));
                intUsed++;
            }


            return strReturn;
        }

        public String DateString()
        {
            return String.Format("Year {0},Day {1}, {2}h, {3}m, {4}s", Year + 1, Day + 1, Hour, Minute, Second);
        }

        public String DateTimeString()
        {
            return String.Format("Year {0},Day {1}, {2:00}:{3:00}:{4:00}", Year + 1, Day + 1, Hour, Minute, Second);
        }

        public String IntervalStringLong()
        {
            String strReturn = "";
            if (UT < 0) strReturn += "+ ";
            strReturn += String.Format("{0} Years, {1} Days, {2:00}:{3:00}:{4:00}", Math.Abs(Year), Math.Abs(Day), Math.Abs(Hour), Math.Abs(Minute), Math.Abs(Second));
            return strReturn;
        }

        public String UTString()
        {
            String strReturn = "";
            if (UT < 0) strReturn += "+ ";
            strReturn += String.Format("{0:N0}s", Math.Abs(UT));
            return strReturn;
        }
        #endregion

        public override String ToString()
        {
            return IntervalStringLong();
        }

        #region Static Properties
        public Double HoursPerDay { get { return GameSettings.KERBIN_TIME ? HoursPerDayKerbin : HoursPerDayEarth; } }
        public Double HoursPerYear { get { return GameSettings.KERBIN_TIME ? HoursPerYearKerbin : HoursPerYearEarth; } }
        public Double DaysPerYear { get { return HoursPerYear / HoursPerDay; } }
        #endregion

        #region "Static Functions"
        //fudging for dates
        public static KSPTime timeDateOffest = new KSPTime(1, 1, 0, 0, 0);

        public static Double BuildUTFromRaw(String Years, String Days, String Hours, String Minutes, String Seconds)
        {
            return BuildUTFromRaw(Convert.ToDouble(Years), Convert.ToDouble(Days), Convert.ToDouble(Hours), Convert.ToDouble(Minutes), Convert.ToDouble(Seconds));
        }
        public static Double BuildUTFromRaw(double Years, double Days, double Hours, double Minutes, double Seconds)
        {
            if (GameSettings.KERBIN_TIME) {
                return Seconds +
                   Minutes * 60 +
                   Hours * 60 * 60 +
                   Days * HoursPerDayKerbin * 60 * 60 +
                   Years * HoursPerYearKerbin * 60 * 60;
            }
            else {
                return Seconds +
                   Minutes * 60 +
                   Hours * 60 * 60 +
                   Days * HoursPerDayEarth * 60 * 60 +
                   Years * HoursPerYearEarth * 60 * 60;
            }
        }

        public static String PrintInterval(KSPTime timeTemp, KSPTime.PrintTimeFormat TimeFormat)
        {
            return PrintInterval(timeTemp, 3, TimeFormat);
        }

        public static String PrintInterval(KSPTime timeTemp, int Segments, KSPTime.PrintTimeFormat TimeFormat)
        {
            switch (TimeFormat) {
                case PrintTimeFormat.TimeAsUT:
                    return timeTemp.UTString();
                case PrintTimeFormat.KSPString:
                    return timeTemp.IntervalString(Segments);
                case PrintTimeFormat.DateTimeString:
                    return timeTemp.IntervalDateTimeString(Segments);
                default:
                    return timeTemp.IntervalString(Segments);
            }
        }

        public static String PrintDate(KSPTime timeTemp, KSPTime.PrintTimeFormat TimeFormat)
        {
            switch (TimeFormat) {
                case PrintTimeFormat.TimeAsUT:
                    return timeTemp.UTString();
                case PrintTimeFormat.KSPString:
                    return timeTemp.DateString();
                case PrintTimeFormat.DateTimeString:
                    return timeTemp.DateTimeString();
                default:
                    return timeTemp.DateTimeString();
            }
        }

        public enum PrintTimeFormat
        {
            TimeAsUT,
            KSPString,
            DateTimeString
        }
        #endregion
    }

    public class KSPTimeStringArray
    {
        public enum TimeEntryFieldsEnum
        {
            Seconds = 0,
            Minutes = 1,
            Hours = 2,
            Days = 3,
            Years = 4
        }

        public TimeEntryFieldsEnum TimeEntryPrecision { get; private set; }

        private String _Years = "", _Days = "", _Hours = "", _Minutes = "", _Seconds = "";

        public String Years { get { return _Years; } set { _Years = value; SetValid(); } }
        public String Days { get { return _Days; } set { _Days = value; SetValid(); } }
        public String Hours { get { return _Hours; } set { _Hours = value; SetValid(); } }
        public String Minutes { get { return _Minutes; } set { _Minutes = value; SetValid(); } }
        public String Seconds { get { return _Seconds; } set { _Seconds = value; SetValid(); } }

        public Boolean Valid { get { return _Valid; } }
        Boolean _Valid = true;

        public KSPTimeStringArray(TimeEntryFieldsEnum LevelOfPrecision)
        {
            TimeEntryPrecision = LevelOfPrecision;
        }
        public KSPTimeStringArray(Double NewUT, TimeEntryFieldsEnum LevelOfPrecision)
            : this(LevelOfPrecision)
        {
            BuildFromUT(NewUT);
        }

        private void SetValid()
        {
            Double dblTest;
            if (Double.TryParse(_Years, out dblTest) &&
                Double.TryParse(_Days, out dblTest) &&
                Double.TryParse(_Hours, out dblTest) &&
                Double.TryParse(_Minutes, out dblTest) &&
                Double.TryParse(_Seconds, out dblTest)
                )
            {
                _Valid = true;
            }
            else
            {
                _Valid = false;
            }
        }

        public void BuildFromUT(Double UT)
        {
            KSPTime timeTemp = new KSPTime(UT);
            if (TimeEntryPrecision >= TimeEntryFieldsEnum.Years)
                Years = timeTemp.Year.ToString();
            else
                Years = "0";

            if (TimeEntryPrecision > TimeEntryFieldsEnum.Days)
                Days = timeTemp.Day.ToString();
            else if (TimeEntryPrecision == TimeEntryFieldsEnum.Days)
                Days = ((timeTemp.Year * timeTemp.DaysPerYear) + timeTemp.Day).ToString();
            else
                Days = "0";

            if (TimeEntryPrecision > TimeEntryFieldsEnum.Hours)
                Hours = timeTemp.Hour.ToString();
            else if (TimeEntryPrecision == TimeEntryFieldsEnum.Hours)
                Hours = ((timeTemp.Year * timeTemp.HoursPerYear) + (timeTemp.Day * timeTemp.HoursPerDay) + timeTemp.Hour).ToString();
            else
                Hours = "0";

            Minutes = timeTemp.Minute.ToString();
            Seconds = timeTemp.Second.ToString();
        }

        public double UT
        {
            get
            {
                return KSPTime.BuildUTFromRaw(
                    ZeroString(Years),
                    ZeroString(Days),
                    ZeroString(Hours),
                    ZeroString(Minutes),
                    ZeroString(Seconds)
                    );
            }
        }
        private String ZeroString(String strInput)
        {
            Double dblTemp;
            if (!Double.TryParse(strInput, out dblTemp))
                return "0";
            else
                return strInput;
        }
    }
}