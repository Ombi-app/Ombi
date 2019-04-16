using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers
{
    public static class PaginationHelper
    {
        public static List<PagesToLoad> GetNextPages(int currentlyLoaded, int toTake, int maxItemsPerPage)
        {
            var result = new List<PagesToLoad>();

            var firstPage = currentlyLoaded / maxItemsPerPage + 1;
            var startPos = currentlyLoaded % maxItemsPerPage + 1;

            var lastItemIndex = currentlyLoaded + toTake - 1;
            var lastPage = lastItemIndex / maxItemsPerPage + 1;
            var stopPos = lastItemIndex % maxItemsPerPage + 1;

            if (currentlyLoaded > maxItemsPerPage)
            {
                currentlyLoaded = currentlyLoaded - maxItemsPerPage;
            }

            var page1 = new PagesToLoad {Page = firstPage, Skip = currentlyLoaded, Take = toTake};

            if (toTake + startPos - 1 > maxItemsPerPage)
            {
                page1.Take = maxItemsPerPage - startPos + 1;
                result.Add(page1);

                for (var i = firstPage + 1; i < lastPage; i++)
                {
                    var nextPage = new PagesToLoad {Page = i, Skip = 0, Take = maxItemsPerPage};
                    result.Add(nextPage);
                }

                var pageN = new PagesToLoad {Page = lastPage, Skip = 0, Take = stopPos};
                result.Add(pageN);
            }
            else
            {
                result.Add(page1);
            }

            return result;
        }
    }

    public class PagesToLoad
    {
        public int Page { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
    }
}
