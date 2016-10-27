using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Ninject;
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

        public void MigrateToLatest()
        {
            var con = Db.DbConnection();
            var versions = GetMigrations().OrderBy(x => x.Key);
            
            var dbVersion = con.GetVersionInfo().OrderByDescending(x => x.Version).FirstOrDefault();
            if (dbVersion == null)
            {
                dbVersion = new TableCreation.VersionInfo { Version = 0 };
            }
            foreach (var v in versions)
            {
                if (v.Value.Version > dbVersion.Version)
                {
                    // Assuming only one constructor
                    var ctor = v.Key.GetConstructors().FirstOrDefault();
                    var dependencies = new List<object>();

                    foreach (var param in ctor.GetParameters())
                    {
                        Console.WriteLine(string.Format(
                            "Param {0} is named {1} and is of type {2}",
                            param.Position, param.Name, param.ParameterType));

                        var dep = Kernel.Get(param.ParameterType);
                        dependencies.Add(dep);
                    }

                    var method = v.Key.GetMethod("Start");
                    if (method != null)
                    {
                        object result = null;
                        var classInstance = Activator.CreateInstance(v.Key, dependencies.Any() ? dependencies.ToArray() : null);

                        var parametersArray = new object[] { Db.DbConnection() };

                        method.Invoke(classInstance, parametersArray);
                    }
                }
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
