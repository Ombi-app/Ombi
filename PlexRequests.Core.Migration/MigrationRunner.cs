using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using PlexRequests.Store;

namespace PlexRequests.Core.Migration
{
    public class MigrationRunner : IMigrationRunner
    {
        public MigrationRunner(ISqliteConfiguration db)
        {
            Db = db;
        }

        private ISqliteConfiguration Db { get; }

        public void MigrateToLatest()
        {
            var con = Db.DbConnection();
            var versions = GetMigrations().OrderBy(x => x.Key);

            var dbVersion = con.GetSchemaVersion();

            foreach (var v in versions)
            {
                if (v.Value.Version > dbVersion.SchemaVersion)
                {
                    var method = v.Key.GetMethod("Start");
                    if (method != null)
                    {
                        object result = null;
                        var classInstance = Activator.CreateInstance(v.Key, null);

                        var parametersArray = new object[] { Db.DbConnection() };

                        method.Invoke(classInstance, parametersArray);
                    }
                }
            }
        }

        private Dictionary<Type, MigrationModel> GetMigrations()
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

        private class MigrationModel
        {
            public int Version { get; set; }
            public string Description { get; set; }
        }
    }
}
