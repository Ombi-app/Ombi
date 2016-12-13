using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Ninject;
using NLog;
using PlexRequests.Store;

namespace PlexRequests.Core.Migration
{
    public class MigrationRunner : IMigrationRunner
    {
        public MigrationRunner(ISqliteConfiguration db, IKernel kernel)
        {
            Db = db;
            Kernel = kernel;
        }

        private IKernel Kernel { get; }
        private ISqliteConfiguration Db { get; }
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public void MigrateToLatest()
        {
            var con = Db.DbConnection();
            var versions = GetMigrations();

            var dbVersion = con.GetVersionInfo().OrderByDescending(x => x.Version).FirstOrDefault() ??
                            new TableCreation.VersionInfo {Version = 0};
            foreach (var v in versions)
            {
#if !DEBUG
                if (v.Value.Version > dbVersion.Version)
                {
#endif
                try
                {
                    // Assuming only one constructor
                    var ctor = v.Key.GetConstructors().FirstOrDefault();
                    var dependencies = ctor.GetParameters().Select(param => Kernel.Get(param.ParameterType)).ToList();

                    var method = v.Key.GetMethod("Start");
                    if (method != null)
                    {
                        var classInstance = Activator.CreateInstance(v.Key, dependencies.Any() ? dependencies.ToArray() : null);
                        var parametersArray = new object[] { Db.DbConnection() };

                        method.Invoke(classInstance, parametersArray);
                    }
                }
                catch (Exception e)
                {
                    _log.Fatal("Error when migrating");
                    _log.Fatal(e);
                }

#if !DEBUG
                }
#endif
            }
        }

        public static Dictionary<Type, MigrationModel> GetMigrations()
        {
            var migrationTypes = GetTypesWithHelpAttribute(Assembly.GetAssembly(typeof(MigrationRunner)));

            var version = new Dictionary<Type, MigrationModel>();

            foreach (var t in migrationTypes)
            {
                var customAttributes = (Migration[])t.GetCustomAttributes(typeof(Migration), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    version.Add(t, new MigrationModel { Version = attr.Version, Description = attr.Description });
                }
            }
            return version;
        }

        private static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(Migration), true).Length > 0);
        }

        public class MigrationModel
        {
            public int Version { get; set; }
            public string Description { get; set; }
        }
    }
}
