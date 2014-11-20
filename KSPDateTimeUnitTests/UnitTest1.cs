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
            
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(1, dt.DayOfYear);
            Assert.AreEqual(1, dt.Year);

            dt = dt.AddMilliSeconds(456);
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(579, dt.Millisecond);

            dt = new KSPDateTime(2, 50, 0, 6, 8,456);
            Assert.AreEqual(6, dt.Minute);
            Assert.AreEqual(8, dt.Second);
            Assert.AreEqual(456, dt.Millisecond);
            Assert.AreEqual(2, dt.Year,"Hello");
            Assert.AreEqual(50, dt.DayOfYear);

        }

        [TestMethod]
        public void TestEarthDateTime()
        {
            KSPDateTimeStructure.SetEarthCalendar();
            Double DateUT = 301.123;
            KSPDateTime dt = new KSPDateTime(DateUT);
            //Console.Write(dt.Day);

            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(1, dt.DayOfYear);
            Assert.AreEqual(1951, dt.Year);

            dt = dt.AddMilliSeconds(456);
            Assert.AreEqual(5, dt.Minute);
            Assert.AreEqual(1, dt.Second);
            Assert.AreEqual(579, dt.Millisecond);

            //dt.Second = 68;
            //Assert.AreEqual(6, dt.Minute);
            //Assert.AreEqual(8, dt.Second);
            //Assert.AreEqual(456, dt.Millisecond);


            //dt.Year = 1969;
            //Assert.AreEqual(1969, dt.Year, "Hello");
            //dt.DayOfYear = 50;
            //Assert.AreEqual(50, dt.DayOfYear);

            //KSPDateTimeStructure.SetCalendarTypeEarth(1951,1,1);
            //dt = new KSPDateTime(1951, 50, 10, 20, 30);
            //Assert.AreEqual(2, dt.Month);
            //Assert.AreEqual(19, dt.Day);

        }


        [TestMethod]
        public void TestMonths()
        {
            KSPDateTimeStructure.SetCustomCalendar();

            //empty months structure
            KSPDateTime dt = new KSPDateTime(1, 100);
            Assert.AreEqual(0, dt.Month);
            Assert.AreEqual(100, dt.Day);

            //set up some months
            KSPDateTimeStructure.Months.Add(new KSPMonth("Billtempber", 200));
            KSPDateTimeStructure.Months.Add(new KSPMonth("Jebuary", 265));

            Assert.AreEqual(1, dt.Month);
            Assert.AreEqual(100, dt.Day);
            dt = dt.AddDays(100);
            Assert.AreEqual(1, dt.Month);
            Assert.AreEqual(200, dt.Day);
            dt = dt.AddDays(100);
            Assert.AreEqual(2, dt.Month);
            dt = dt.AddDays(100);
        }

        [TestMethod]
        public void TestAbstract()
        {

            Double DateUT = 301.123;

            KSPDateTime20 dt = new KSPDateTime20();
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


            KSPTimeSpan20 a = new KSPTimeSpan20();
            a.UT = 300;
            a.Second = 32;

        }
    }
}
