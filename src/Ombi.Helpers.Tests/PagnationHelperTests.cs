using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class PaginationHelperTests
    {
        [TestCaseSource(nameof(TestPageData))]
        public void TestPaginationPages(int currentlyLoaded, int toLoad, int maxItemsPerPage, int[] expectedPages)
        {
            var result = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, maxItemsPerPage);
            var pages = result.Select(x => x.Page).ToArray();

            Assert.That(pages.Length, Is.EqualTo(expectedPages.Length), "Did not contain the correct amount of pages");
            for (var i = 0; i < pages.Length; i++)
            {
                Assert.That(pages[i], Is.EqualTo(expectedPages[i]));
            }
        }
        
        public static IEnumerable<TestCaseData> TestPageData
        {
            get
            {
                yield return new TestCaseData(0, 10, 20,  new [] { 1 }).SetName("Pagination_Load_First_Page");
                yield return new TestCaseData(20, 10, 20, new [] { 2 }).SetName("Pagination_Load_Second_Page");
                yield return new TestCaseData(0, 20, 20,  new [] { 1 }).SetName("Pagination_Load_Full_First_Page");
                yield return new TestCaseData(20, 20, 20, new [] { 2 }).SetName("Pagination_Load_Full_Second_Page");
                yield return new TestCaseData(10, 20, 20, new [] { 1, 2 }).SetName("Pagination_Load_Half_First_Page_And_Half_Second_Page");
                yield return new TestCaseData(19, 20, 20, new[] { 1, 2 }).SetName("Pagination_Load_End_First_Page_And_Most_Second_Page");
                yield return new TestCaseData(19, 40, 20, new[] { 1, 2, 3 }).SetName("Pagination_Load_End_First_Page_And_Most_Second_And_Third_Page");
                yield return new TestCaseData(10, 10, 20, new[] { 1 }).SetName("Pagination_Load_Half_First_Page");
                yield return new TestCaseData(10, 9, 20, new[] { 1 }).SetName("Pagination_Load_LessThan_Half_First_Page");
                yield return new TestCaseData(20, 10, 20, new[] { 2 }).SetName("Pagination_Load_Half_Second_Page");
                yield return new TestCaseData(20, 9, 20, new[] { 2 }).SetName("Pagination_Load_LessThan_Half_Second_Page");
                yield return new TestCaseData(30, 10, 20, new[] { 2 }).SetName("Pagination_Load_All_Second_Page_With_Half_Take");
                yield return new TestCaseData(49, 1, 50, new[] { 1 }).SetName("Pagination_Load_49_OutOf_50");
                yield return new TestCaseData(49, 1, 100,new[] { 1 }).SetName("Pagination_Load_50_OutOf_100");
            }
        }

        [TestCaseSource(nameof(CurrentPositionTestData))]
        public void TestCurrentPositionOfPagination(int currentlyLoaded, int toLoad, int maxItemsPerPage, int expectedTake, int expectedSkip)
        {
            var result = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, maxItemsPerPage);

            var first = result.FirstOrDefault();
            Assert.That(first.Take, Is.EqualTo(expectedTake));
            Assert.That(first.Skip, Is.EqualTo(expectedSkip));
        }
        public static IEnumerable<TestCaseData> CurrentPositionTestData
        {
            get
            {
                yield return new TestCaseData(0, 10, 20, 10, 0).SetName("PaginationPosition_Load_First_Half_Of_Page");
                yield return new TestCaseData(10, 10, 20, 10, 10).SetName("PaginationPosition_Load_EndHalf_First_Page");
                yield return new TestCaseData(19, 1, 20, 1, 19).SetName("PaginationPosition_Load_LastItem_Of_First_Page");
                yield return new TestCaseData(20, 20, 300, 20, 20).SetName("PaginationPosition_Load_Full_Second_Page");
            }
        }

        [TestCaseSource(nameof(CurrentPositionMultiplePagesTestData))]
        public void TestCurrentPositionOfPaginationWithMultiplePages(int currentlyLoaded, int toLoad, int maxItemsPerPage, List<MultiplePagesTestData> data)
        {
            var result = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, maxItemsPerPage);

            foreach (var r in result)
            {
                // get result data for this page
                var expectedPage = data.FirstOrDefault(x => x.Page == r.Page);
                Assert.That(r.Take, Is.EqualTo(expectedPage.ExpectedTake));
                Assert.That(r.Skip, Is.EqualTo(expectedPage.ExpectedSkip));
            }
        }

        public static IEnumerable<TestCaseData> CurrentPositionMultiplePagesTestData
        {
            get
            {
                yield return new TestCaseData(10, 20, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(1, 10, 10), new MultiplePagesTestData(2, 10, 0) })
                    .SetName("PaginationPosition_Load_SecondHalf_FirstPage_FirstHalf_SecondPage");
                yield return new TestCaseData(0, 40, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(1, 20, 0), new MultiplePagesTestData(2, 20, 0) })
                    .SetName("PaginationPosition_Load_Full_First_And_SecondPage");
                yield return new TestCaseData(35, 15, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(2, 5, 15), new MultiplePagesTestData(3, 10, 0) })
                    .SetName("PaginationPosition_Load_EndSecondPage_Beginning_ThirdPage");
                yield return new TestCaseData(18, 22, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(1, 2, 18), new MultiplePagesTestData(2, 20, 0) })
                    .SetName("PaginationPosition_Load_EndFirstPage_Full_SecondPage");
                yield return new TestCaseData(38, 4, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(2, 2, 18), new MultiplePagesTestData(3, 2, 0) })
                    .SetName("PaginationPosition_Load_EndSecondPage_Some_ThirdPage"); 
                yield return new TestCaseData(15, 20, 10, new List<MultiplePagesTestData> { new MultiplePagesTestData(2, 5, 5), new MultiplePagesTestData(3, 10, 0), new MultiplePagesTestData(4, 5, 0) })
                    .SetName("PaginationPosition_Load_EndSecondPage_All_ThirdPage_Some_ForthPage");
                yield return new TestCaseData(24, 12, 12, new List<MultiplePagesTestData> { new MultiplePagesTestData(3, 12, 0) })
                    .SetName("PaginationPosition_Load_ThirdPage_Of_12");
                yield return new TestCaseData(12, 12, 12, new List<MultiplePagesTestData> { new MultiplePagesTestData(2, 12, 0) })
                    .SetName("PaginationPosition_Load_SecondPage_Of_12");
                yield return new TestCaseData(40, 20, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(3, 20, 0) })
                    .SetName("PaginationPosition_Load_FullThird_Page");
                yield return new TestCaseData(240, 12, 20, new List<MultiplePagesTestData> { new MultiplePagesTestData(13, 12, 0) })
                    .SetName("PaginationPosition_Load_Page_13");
            }
        }
         
        public class MultiplePagesTestData
        {
            public MultiplePagesTestData(int page, int take, int skip)
            {
                Page = page;
                ExpectedTake = take;
                ExpectedSkip = skip;
            }
            public int Page { get; set; }
            public int ExpectedTake { get; set; }
            public int ExpectedSkip { get; set; }
        }


    }
}
