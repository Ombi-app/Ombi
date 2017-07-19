using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using System;
using System.Threading.Tasks;

namespace Ombi.Store.Repository
{
    //public class TokenRepository : ITokenRepository
    //{
    //    public TokenRepository(IOmbiContext db)
    //    {
    //        Db = db;
    //    }

    //    private IOmbiContext Db { get; }

    //    public async Task CreateToken(EmailTokens token)
    //    {
    //        token.Token = Guid.NewGuid();
    //        await Db.EmailTokens.AddAsync(token);
    //        await Db.SaveChangesAsync();
    //    }

    //    public async Task<EmailTokens> GetToken(Guid tokenId)
    //    {
    //        return await Db.EmailTokens.FirstOrDefaultAsync(x => x.Token == tokenId);
    //    }
    //}
}
