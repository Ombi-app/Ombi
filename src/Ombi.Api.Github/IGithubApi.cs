using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.Github.Models;

namespace Ombi.Api.Github
{
    public interface IGithubApi
    {
        Task<List<CakeThemes>> GetCakeThemes();
    }
}