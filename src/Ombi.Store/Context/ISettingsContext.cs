using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public interface ISettingsContext : IDbContext
    {
        DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }
        DbSet<GlobalSettings> Settings { get; set; }
    }
}