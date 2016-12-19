#region Copyright
//  ***********************************************************************
//  Copyright (c) 2016 Jamie Rees
//  File: TableCreation.cs
//  Created By: Jamie Rees
//
//  Permission is hereby granted, free of charge, to any person obtaining
//  a copy of this software and associated documentation files (the
//  "Software"), to deal in the Software without restriction, including
//  without limitation the rights to use, copy, modify, merge, publish,
//  distribute, sublicense, and/or sell copies of the Software, and to
//  permit persons to whom the Software is furnished to do so, subject to
//  the following conditions:
//
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ***********************************************************************
#endregion

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Ombi.Store
{
    public static class TableCreation
    {
        /// <summary>
        /// Creates the tables located in the SqlTables.sql file.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public static void CreateTables(this IDbConnection connection)
        {
            connection.Open();
            connection.Execute(Sql.SqlTables);
            connection.Close();
        }

        public static void DropTable(this IDbConnection con, string tableName)
        {
            using (con)
            {
                con.Open();
                var query = $"DROP TABLE IF EXISTS {tableName}";
                con.Execute(query);
                con.Close();
            }
        }

        public static void AlterTable(this IDbConnection connection, string tableName, string alterType, string newColumn, bool allowNulls, string dataType)
        {
            connection.Open();
            var result = connection.Query<TableInfo>($"PRAGMA table_info({tableName});");
            if (result.Any(x => x.name == newColumn))
            {
                connection.Close();
                return;
            }

            var query = $"ALTER TABLE {tableName} {alterType} {newColumn} {dataType}";
            if (!allowNulls)
            {
                query = query + " NOT NULL";
            }

            connection.Execute(query);

            connection.Close();
        }

        public static void Vacuum(this IDbConnection con)
        {
            using (con)
            {
                con.Open();

                con.Query("VACUUM;");

            }
        }

        public static DbInfo GetSchemaVersion(this IDbConnection con)
        {
            con.Open();
            var result = con.Query<DbInfo>("SELECT * FROM DBInfo");
            con.Close();

            return result.FirstOrDefault();
        }

        public static void UpdateSchemaVersion(this IDbConnection con, int version)
        {
            con.Open();
            con.Query($"UPDATE DBInfo SET SchemaVersion = {version}");
            con.Close();
        }

        public static void CreateSchema(this IDbConnection con, int version)
        {
            con.Open();
            con.Query($"INSERT INTO DBInfo (SchemaVersion) values ({version})");
            con.Close();
        }

        public static IEnumerable<VersionInfo> GetVersionInfo(this IDbConnection con)
        {
            con.Open();
            var result = con.Query<VersionInfo>("SELECT * FROM VersionInfo");
            con.Close();

            return result;
        }

        public static void AddVersionInfo(this IDbConnection con, VersionInfo ver)
        {
            con.Open();
            con.Insert(ver);
            con.Close();
        }


        [Table("VersionInfo")]
        public class VersionInfo
        {
            public int Version { get; set; }
            public string Description { get; set; }
        }

        [Table("DBInfo")]
        public class DbInfo
        {
            public int SchemaVersion { get; set; }
        }

        [Table("sqlite_master")]
        public class SqliteMasterTable
        {
            public string type { get; set; }
            public string name { get; set; }
            public string tbl_name { get; set; }
            [Key]
            public long rootpage { get; set; }
            public string sql { get; set; }
        }

        [Table("table_info")]
        public class TableInfo
        {
            public int cid { get; set; }
            public string name { get; set; }
            public int notnull { get; set; }
            public string dflt_value { get; set; }
            public int pk { get; set; }
        }
    }
}
