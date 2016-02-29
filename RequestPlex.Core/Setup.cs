using Mono.Data.Sqlite;

using RequestPlex.Store;

namespace RequestPlex.Core
{
    public class Setup
    {

        public void SetupDb()
        {
            var db = new DbConfiguration(new SqliteFactory());
            db.CheckDb();
            TableCreation.CreateTables(db.DbConnection());
        }
    }
}
