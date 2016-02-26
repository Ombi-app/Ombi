using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestPlex.Api
{
    public abstract class MovieBase
    {
        protected string ApiKey = "b8eabaf5608b88d0298aa189dd90bf00";
        protected string Url = "http://api.themoviedb.org/3";
    }
}
