using System.Data.SQLite;

namespace Flats
{
    internal class FlatDbConnection : IDisposable
    {
        public FlatDbConnection()
        {
            Sqlite = createConnection();
        }

        public SQLiteConnection Sqlite { get; set; }

        public void Dispose()
        {
            Sqlite.Close();
            Sqlite.Dispose();  
            Sqlite = null;
        }

        private SQLiteConnection createConnection()
        {
            var connection = new SQLiteConnection();
            connection.ConnectionString = "Data Source=Flat.db";
            connection.Open();
            
            return connection;
        }
    }
}
