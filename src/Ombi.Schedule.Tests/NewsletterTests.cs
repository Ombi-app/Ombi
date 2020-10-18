using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Ombi;
using static Ombi.Schedule.Jobs.Ombi.NewsletterJob;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class NewsletterTests
    {
        [TestCaseSource(nameof(EpisodeListData))]
        public string BuildEpisodeListTest(List<int> episodes)
        {
            var ep = new List<int>();
            foreach (var i in episodes)
            {
                ep.Add(i);
            }
            var result = StringHelper.BuildEpisodeList(ep);
            return result;
        }

        public static IEnumerable<TestCaseData> EpisodeListData
        {
            get
            {
                yield return new TestCaseData(new List<int>{1,2,3,4,5,6}).Returns("1-6").SetName("Simple 1-6");
                yield return new TestCaseData(new List<int>{1,2,3,4,5,6,8,9}).Returns("1-6, 8-9").SetName("Simple 1-6, 8-9");
                yield return new TestCaseData(new List<int>{1,99,101,555,468,469}).Returns("1, 99, 101, 555, 468-469").SetName("More Complex");
                yield return new TestCaseData(new List<int>{1}).Returns("1").SetName("Single Episode");
            }
        }

        [Test, TestCaseSource(nameof(EpisodeAndDateListData))]
        public string BuildEpisodeListTest(List<(int, DateTime?)> episodes)
        {
            var ep = new List<(int, DateTime?)>();
            foreach (var i in episodes)
            {
                ep.Add(i);
            }
            var result = BuildEpisodeList(ep);
            return result;
        }

        static IEnumerable<TestCaseData> EpisodeAndDateListData
        {
            get
            {
                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, (DateTime?)null), }).Returns("1").SetName("Single episode date is null ");

                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, new DateTime(2020, 01, 01)), }).Returns("1 01/01/2020").SetName("Simple With Date Single episode");

                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, new DateTime(2020, 01, 01)),
                    (2, new DateTime(2020, 01, 02)),
                    (3, new DateTime(2020, 01, 03)),
                    (4, new DateTime(2020, 01, 04)),
                    (5, new DateTime(2020, 01, 05)),
                    (6, new DateTime(2020, 01, 06)) }).Returns("1-6 01/2020").SetName("Simple With Date 1-6");

                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, new DateTime(2020, 01, 01)),
                    (2, new DateTime(2020, 01, 02)),
                    (3, new DateTime(2020, 01, 03)),
                    (4, new DateTime(2020, 01, 04)),
                    (5, new DateTime(2020, 01, 05)),
                    (6, new DateTime(2020, 01, 06)),
                    (8, new DateTime(2020, 02, 08)),
                    (9, new DateTime(2020, 02, 09)),}).Returns("1-6 01/2020, 8-9 02/2020").SetName("Simple With Date 1-6, 8-9");

                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, new DateTime(2020, 01, 01)),
                    (2, new DateTime(2020, 01, 02)),
                    (3, new DateTime(2020, 01, 03)),
                    (4, new DateTime(2020, 01, 04)),
                    (5, new DateTime(2020, 01, 05)),
                    (6, new DateTime(2020, 01, 06)),
                    (8, new DateTime(2020, 01, 08)),
                    (9, new DateTime(2020, 02, 09)),}).Returns("1-6 01/2020, 8-9 02/2020").SetName("Simple With Date 1-6, 8-9 overlapping month");


                yield return new TestCaseData(new List<(int, DateTime?)> {
                    (1, new DateTime(2020, 01, 01)),
                    (99, new DateTime(2020, 02, 27)),
                    (101, new DateTime(2020, 03, 15)),
                    (555, new DateTime(2020, 05, 04)),
                    (468, new DateTime(2020, 06, 05)),
                    (469, new DateTime(2020, 06, 12)) }).Returns("1 01/2020, 99 02/2020, 101 03/2020, 555 05/2020, 468-469 06/2020").SetName("More Complex with dates");
            }
        }
    }
}