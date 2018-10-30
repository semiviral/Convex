#region USINGS

using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Convex.Event;
using Microsoft.Data.Sqlite;
using Serilog.Events;

#endregion

namespace Convex.IRC.Component {
    public class Database {
        /// <summary>
        ///     Initialise connections to database and sets properties
        /// </summary>
        public Database(string databaseFilePath) {
            FilePath = databaseFilePath;
        }

        #region MEMBERS

        public string FilePath { get; }
        public bool Connected { get; private set; }
        public bool IsInitialised { get; private set; }

        #endregion

        #region INIT

        public async Task Initialise() {
            await CheckCreate();

            IsInitialised = true;
            await OnInitialised(this, new ClassInitialisedEventArgs(this));
        }

        private async Task CheckCreate() {
            if (File.Exists(FilePath)) {
                return;
            }

            await LogInformation(this, new LogEventArgs(LogEventLevel.Information, "Database file not found, creating."));

            using (SqliteConnection connection = GetConnection(FilePath, SqliteOpenMode.ReadWriteCreate)) {
                connection.Open();
                await connection.QueryAsync(new DatabaseQueriedEventArgs("CREATE TABLE IF NOT EXISTS messages (id int, sender string, message string, datetime string)"));
            }

            Connected = true;
        }

        private static SqliteConnection GetConnection(string source, SqliteOpenMode mode = SqliteOpenMode.ReadWrite) {
            return new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = source, Mode = mode }.ToString());
        }

        #endregion


        #region EVENTS

        public event AsyncEventHandler<ClassInitialisedEventArgs> Initialised;
        public event AsyncEventHandler<LogEventArgs> Log;

        private async Task OnInitialised(object sender, ClassInitialisedEventArgs args) {
            if (Initialised == null) {
                return;
            }

            await Initialised.Invoke(sender, args);
        }

        private async Task LogInformation(object sender, LogEventArgs args) {
            if (Log == null) {
                return;
            }

            await Log.Invoke(sender, args);
        }

        private void MessageAdded(object sender, NotifyCollectionChangedEventArgs args) {
            if (!args.Action.Equals(NotifyCollectionChangedAction.Add)) {
                return;
            }

            foreach (object item in args.NewItems) {
                if (!(item is Message)) {
                    continue;
                }

                Message message = (Message)item;

                GetConnection(FilePath).Query(new DatabaseQueriedEventArgs($"INSERT INTO messages VALUES ({message.Id}, '{message.Sender}', '{message.Contents}', '{message.Date}')"));
            }
        }

        private void AutoUpdateUsers(object sender, PropertyChangedEventArgs args) {
            if (!(args is UserPropertyChangedEventArgs)) {
                return;
            }

            UserPropertyChangedEventArgs castedArgs = (UserPropertyChangedEventArgs)args;

            GetConnection(FilePath).Query(new DatabaseQueriedEventArgs($"UPDATE users SET {castedArgs.PropertyName}='{castedArgs.NewValue}' WHERE realname='{castedArgs.Name}'"));
        }

        #endregion
    }
}