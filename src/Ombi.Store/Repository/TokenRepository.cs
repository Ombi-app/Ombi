using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Helpers;

namespace Ombi.Store.Repository
{
    public class TokenRepository : BaseRepository<Tokens, OmbiContext>, ITokenRepository
    {
        public TokenRepository(OmbiContext db) : base(db)
        {
            Db = db;
        }

        private OmbiContext Db { get; }

        public async Task CreateToken(Tokens token)
        {
            await Db.Tokens.AddAsync(token);
            await InternalSaveChanges();
        }

        public IQueryable<Tokens> GetToken(string tokenId)
        {
            return Db.Tokens.Where(x => x.Token == tokenId);
        }
    }
}
