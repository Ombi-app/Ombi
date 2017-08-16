using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Store.Repository
{
    public class TokenRepository : ITokenRepository
    {
        public TokenRepository(IOmbiContext db)
        {
            Db = db;
        }

        private IOmbiContext Db { get; }

        public async Task CreateToken(Tokens token)
        {
            await Db.Tokens.AddAsync(token);
            await Db.SaveChangesAsync();
        }

        public IQueryable<Tokens> GetToken(string tokenId)
        {
            return Db.Tokens.Where(x => x.Token == tokenId);
        }
    }
}
