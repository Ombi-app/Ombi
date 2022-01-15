using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IMediaServerContentRepository<Content> : IExternalRepository<Content>, IMediaServerContentRepositoryLight
      where Content : IMediaServerContent
    {
    }
}