using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers
{
    public static class PaginationHelper
    {
        public static List<PagesToLoad> GetNextPages(int currentlyLoaded, int toLoad, int maxItemsPerPage)
        {
            var pagesToLoad = new List<PagesToLoad>();

            if (currentlyLoaded == maxItemsPerPage)
            {
                currentlyLoaded++;
            }

            var a = currentlyLoaded / maxItemsPerPage;
            //var currentPage = Convert.ToInt32(Math.Round((decimal)((decimal)currentlyLoaded / (decimal)maxItemsPerPage), 2, MidpointRounding.AwayFromZero));
            var currentPage = Convert.ToInt32(Math.Ceiling((decimal)((decimal)currentlyLoaded / (decimal)maxItemsPerPage)));
            if (currentlyLoaded < maxItemsPerPage)
            {
                currentPage = 1;
            }

            
            var toBeLoaded = (currentlyLoaded + toLoad)+1;
            //var toBeLoadedPage = Convert.ToInt32(Math.Round((decimal)((decimal)toBeLoaded / (decimal)maxItemsPerPage), 2, MidpointRounding.AwayFromZero));
            var toBeLoadedPage = Convert.ToInt32(Math.Ceiling((decimal)((decimal)toBeLoaded / (decimal)maxItemsPerPage)));
            

            if (currentlyLoaded == 0)
            {
                // If we have not loaded any yet, then we should only load
                // the first page
                currentPage = toBeLoadedPage;
            }
            var extraPageNeeded = (toBeLoadedPage != currentPage);
            if(extraPageNeeded)
            {
                // Add the first page
                pagesToLoad.Add(new PagesToLoad
                {
                    Page = currentPage,
                    Skip = currentlyLoaded
                });
                // Add extra page
                pagesToLoad.Add(new PagesToLoad
                {
                    Page = toBeLoadedPage,
                    Skip = (currentlyLoaded + toLoad) - maxItemsPerPage,
                    Take = toLoad
                });
            }
            else
            {
                pagesToLoad.Add(new PagesToLoad
                {
                    Page = currentPage,
                    Skip = currentlyLoaded,
                    Take = toLoad
                });
            }

            return pagesToLoad;
        }
    }

    public class PagesToLoad
    {
        public int Page { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
    }
}
