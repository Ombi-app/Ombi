using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [TestCaseSource(nameof(DayOfWeekData))]
        public DateTime FirstDateInWeekTests(DateTime input, string culture)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            return input.FirstDateInWeek();
        }

        public static IEnumerable<TestCaseData> DayOfWeekData
        {
            get
            {
                yield return new TestCaseData(new DateTime(2021, 09, 20), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Monday, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 21), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Tuesday, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 22), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Wednesday, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 23), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Thursday, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 24), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Friday, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 25), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Sat, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 26), "en-GB").Returns(new DateTime(2021, 09, 20)).SetName("en-GB Sun, FDOW is Monday");
                yield return new TestCaseData(new DateTime(2021, 09, 20), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Monday, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 21), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Tuesday, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 22), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Wednesday, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 23), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Thursday, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 24), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Friday, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 25), "en-US").Returns(new DateTime(2021, 09, 19)).SetName("en-US Sat, FDOW is Sunday");
                yield return new TestCaseData(new DateTime(2021, 09, 26), "en-US").Returns(new DateTime(2021, 09, 26)).SetName("en-US Sun, FDOW is Sunday");
            }
        }
    }
}
