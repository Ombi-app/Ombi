using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.External.ExternalApis.Github.Models;

namespace Ombi.Api.External.ExternalApis.Github
{
    public interface IGithubApi
    {
        Task<List<CakeThemes>> GetCakeThemes();
    }
}