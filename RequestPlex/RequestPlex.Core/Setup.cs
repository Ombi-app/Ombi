using Mono.Data.Sqlite;

using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class Setup
    {

        public void SetupDb()
        {
            var db = new DbConfiguration(new SqliteFactory());
            db.CreateDatabase();
            TableCreation.CreateTables(db.DbConnection());
        }
    }
}
