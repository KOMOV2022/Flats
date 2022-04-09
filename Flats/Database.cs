using System.Data.SQLite;

namespace Flats
{
    class Database
    {
        public void Book(int idFlat)
        {
            connectDbAddSql($"update Flat set Tenant = '{Program.currentUserLogin}' WHERE id = {idFlat}");
        }

        public string? GetUserPassword(string login)
        {
            string? result = null;
            using (var connection = new FlatDbConnection())
            {
                var sql = $"select * from User where login = '{login}'";
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                            result = reader["password"].ToString();
                }
            }
            return result;
        }

        public void connectDbAdd(int ROOMS, int FLOORE, int FULLSQUARE)
        {
            connectDbAddSql($"INSERT INTO Flat (ROOMS, FLOORE, FULLSQUARE, TENANT) " +
                $"VALUES ({ROOMS}, {FLOORE}, {FULLSQUARE}, null)");
        }

        public void connectDbDel(int id)
        {
            connectDbAddSql($"DELETE FROM Flat WHERE id = '{id}'");
        }

        public string? priznakAdmin()
        {
            string sql = $"SELECT big_boss FROM User WHERE login = '{Program.currentUserLogin}'";
            string? someData = "slave";
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                    while (reader.Read())
                        someData = reader.GetString(0);

            }
            return someData;
        }

        public void showFletss()
        {
            showFletsBySql($"SELECT * FROM Flat ");
        }

        public void showFletsBySqare(int min, int max)
        {
            showFletsBySql($"select *" + $" from Flat " +
                $"where (Tenant = '{Program.currentUserLogin}' OR Tenant is NULL) and " +
                $"FullSquare < {max} and FullSquare > {min} order by id ");

        }

        public void showFletsByUser()
        {
            showFletsBySql($"SELECT * FROM Flat " +
                            $"WHERE Tenant = '{Program.currentUserLogin}' OR Tenant is NULL");
        }

        private void showFletsBySql(string sql)
        {
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows) Console.WriteLine($"ID  ROOMS   FLOORE   FULLSQUARE   TENANT");
                else Console.WriteLine($"Нет таких данных");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["ID"]}\t{reader["rooms"]}\t" +
                       $"{reader["floore"]}\t{reader["FullSquare"]}\t  {reader["Tenant"]}");

                }
            }
        }

        private void connectDbAddSql(string sql)
        {
            using (var connection = new FlatDbConnection())
            {
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }
        }
    }
}
