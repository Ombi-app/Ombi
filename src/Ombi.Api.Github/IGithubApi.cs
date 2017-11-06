using System.Threading.Tasks;
using Ombi.Api.Github.Models;

namespace Ombi.Api.Github
{
    public interface IGithubApi
    {
        Task<CakeThemesContainer> GetCakeThemes();
    }
}