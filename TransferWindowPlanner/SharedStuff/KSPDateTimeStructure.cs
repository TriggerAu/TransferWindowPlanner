using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSPPluginFramework
{
    public static class KSPDateTimeStructure
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

        static KSPDateTimeStructure()
        {
            EpochYear = 1;
            EpochDay = 1;
            SecondsPerMinute = 60;
            MinutesPerHour = 60;
                        
            HoursPerDay = GameSettings.KERBIN_TIME ? 6 : 24;
            DaysPerYear = GameSettings.KERBIN_TIME ? 425 : 365;
        }


        
    }

    public class KSPMonth
    {
        public int Days { get; set; }
        public String Name { get; set; }
    }
}
