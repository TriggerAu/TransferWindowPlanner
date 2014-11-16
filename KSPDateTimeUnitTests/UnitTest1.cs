using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TransferWindowPlanner;
using KSPPluginFramework;

namespace KSPDateTimeUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDateTime()
        {
            Double DateUT = 301.123;

            KSPDateTime dt = new KSPDateTime(DateUT);
            //Console.Write(dt.Day);

            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(1, dt.DayOfYear);
            Assert.AreEqual(1, dt.Year);

            dt.Millisecond = 456;
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);

            dt.Second = 68;
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(8, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);


            dt.Year = 2; 
            Assert.AreEqual(2, dt.Year,"Hello");
            dt.DayOfYear = 50;
            Assert.AreEqual(50, dt.DayOfYear);

        }

        [TestMethod]
        public void TestEarthDateTime()
        {
            KSPDateTimeStructure.SetCalendarType(CalendarTypeEnum.Earth);
            Double DateUT = 301.123;
            KSPDateTime dt = new KSPDateTime(DateUT);
            //Console.Write(dt.Day);

            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(1, dt.DayOfYear);
            Assert.AreEqual(1951, dt.Year);

            dt.Millisecond = 456;
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);

            dt.Second = 68;
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(8, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);


            dt.Year = 1969;
            Assert.AreEqual(1969, dt.Year, "Hello");
            dt.DayOfYear = 50;
            Assert.AreEqual(50, dt.DayOfYear);

        }
        [TestMethod]
        public void TestAbstract()
        {

            Double DateUT = 301.123;

            KSPDateTime2 dt = new KSPDateTime2();
            dt.UT = DateUT;
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(1, dt.Day);
            Assert.AreEqual(1, dt.Year);

            dt.Millisecond = 456;
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);

            dt.Second = 68;
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(8, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);


            dt.Year = 2;
            Assert.AreEqual(2, dt.Year, "Hello");
            dt.Day = 50;
            Assert.AreEqual(50, dt.Day);


            KSPTimeSpan2 a = new KSPTimeSpan2();
            a.UT = 300;
            a.Second = 32;

        }
    }
}
