using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public class KSPTimeSpan
    {

        //Descriptors of DateTime
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Milliseconds { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Years { get; set; }

        /// <summary>
        /// Replaces the normal "Ticks" function. This is Seconds of UT since game time 0
        /// </summary>
        public Double UT { get; set; }

        #region Constructors
        public KSPTimeSpan()
        {

        }
        public KSPTimeSpan(int hour, int minute, int second)
            : this()
        {
            Hours = hour; Minutes = minute; Seconds = second;
        }
        public KSPTimeSpan(int year, int day, int hour, int minute, int second)
            : this(hour, minute,second)
        {
            Years = year; Days=day;
        }
        public KSPTimeSpan(int year, int day, int hour, int minute, int second, int millisecond)
            : this(year, day, hour, minute, second)
        {
            Milliseconds = millisecond;
        }

        public KSPTimeSpan(Double ut)
            : this()
        {

        } 
        #endregion

    }
}
