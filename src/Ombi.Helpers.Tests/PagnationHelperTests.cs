using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class PaginationHelperTests
    {
        [TestCaseSource(nameof(TestData))]
        [Ignore("https://stackoverflow.com/questions/55710966/working-out-how-many-items-to-take-in-a-paginated-list")]
        public void TestPaginationPagesToLoad(int currentlyLoaded, int toLoad, int maxItemsPerPage, int[] expectedPages)
        {
            var result = PaginationHelper.GetNextPages(currentlyLoaded, toLoad, maxItemsPerPage);
            var pages = result.Select(x => x.Page).ToArray();

            Assert.That(pages.Length, Is.EqualTo(expectedPages.Length), "Did not contain the correct amount of pages");
            for (int i = 0; i < pages.Length; i++)
            {
                Assert.That(pages[i], Is.EqualTo(expectedPages[i]));
            }


        }

        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData(0, 10, 20,  new [] { 1 }).SetName("Load_First_Page");
                yield return new TestCaseData(20, 10, 20, new [] { 2 }).SetName("Load_Second_Page");
                yield return new TestCaseData(0, 20, 20,  new [] { 2 }).SetName("Load_Full_First_Page_Should_Get_NextPage");
                yield return new TestCaseData(20, 20, 20, new [] { 3 }).SetName("Load_Full_Second_Page_Should_Get_Next_Page");
                yield return new TestCaseData(10, 20, 20, new [] { 1, 2 }).SetName("Load_Half_First_Page_And_Half_Second_Page");
                yield return new TestCaseData(19, 20, 20, new [] { 1, 2 }).SetName("Load_End_First_Page_And_Most_Second_Page");
            }
        }
    }
}
