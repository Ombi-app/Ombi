using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public interface IOmbiContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        DbSet<RequestBlobs> Requests { get; set; }
        DbSet<GlobalSettings> Settings { get; set; }
        DbSet<User> Users { get; set; }
    }
}