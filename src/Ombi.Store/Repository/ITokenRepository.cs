using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface ITokenRepository
    {
        Task CreateToken(Tokens token);
        IQueryable<Tokens> GetToken(string tokenId);
    }
}