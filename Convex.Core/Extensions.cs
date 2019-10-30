#region

using System.Threading.Tasks;
using Convex.Event;
using Microsoft.Data.Sqlite;

#endregion

namespace Convex.Core
{
    public static class Extensions
    {
        public static async Task QueryAsync(this SqliteConnection source, DatabaseQueriedEventArgs args)
        {
            await source.OpenAsync();

            await using SqliteTransaction transaction = source.BeginTransaction();
            await using (SqliteCommand command = source.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = args.Query;
                await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }

        public static void Query(this SqliteConnection source, DatabaseQueriedEventArgs args)
        {
            source.Open();

            using SqliteTransaction transaction = source.BeginTransaction();
            using (SqliteCommand command = source.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = args.Query;
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
