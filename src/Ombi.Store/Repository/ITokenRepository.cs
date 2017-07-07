using System;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface ITokenRepository
    {
        Task CreateToken(EmailTokens token);
        Task<EmailTokens> GetToken(Guid tokenId);
    }
}