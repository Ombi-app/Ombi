using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Entities;

namespace Ombi.Store.Context
{
    public abstract class SettingsContext : DbContext
    {
        protected SettingsContext(DbContextOptions<SettingsContext> options) : base(options)
        {

        }

        /// <summary>
        /// This allows a sub class to call the base class 'DbContext' non typed constructor
        /// This is need because instances of the subclasses will use a specific typed DbContextOptions
        /// which can not be converted into the parameter in the above constructor
        /// </summary>
        /// <param name="options"></param>
        protected SettingsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<GlobalSettings> Settings { get; set; }
        public DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }



        public void Seed()
        {
            var strat = Database.CreateExecutionStrategy();
            strat.Execute(() =>
            {
                using (var tran = Database.BeginTransaction())
                {
                    // Add the tokens
                    var fanArt = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
                    if (fanArt == null)
                    {
                        ApplicationConfigurations.Add(new ApplicationConfiguration
                        {
                            Type = ConfigurationTypes.FanartTv,
                            Value = "4b6d983efa54d8f45c68432521335f15"
                        });
                        SaveChanges();
                    }

                    var movieDb = ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.FanartTv);
                    if (movieDb == null)
                    {
                        ApplicationConfigurations.Add(new ApplicationConfiguration
                        {
                            Type = ConfigurationTypes.TheMovieDb,
                            Value = "b8eabaf5608b88d0298aa189dd90bf00"
                        });
                        SaveChanges();
                    }

                    var notification =
                        ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Notification);
                    if (notification == null)
                    {
                        ApplicationConfigurations.Add(new ApplicationConfiguration
                        {
                            Type = ConfigurationTypes.Notification,
                            Value = "4f0260c4-9c3d-41ab-8d68-27cb5a593f0e"
                        });
                        SaveChanges();
                    }
                    tran.Commit();
                }
            });
        }
    }
}