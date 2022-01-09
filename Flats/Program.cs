using System.Data.SQLite;

namespace Flats
{
    class Program
    {
        static void showFlets(string sql)
        {
            //todo не показывать квартиры, которые забронированы другими пользователями
            //todo сделать логин уникальным в базе
            //todo режим риэлтора. У него есть полномочия добавлять квартиры
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                //новый вариант (без указания имен стообцов)
                var schema = reader.GetColumnSchemaAsync().Result;
                foreach (var column in schema) Console.Write(column.ColumnName + "\t");
                Console.WriteLine();
                while (reader.Read())
                {
                    var valuesStr = "";
                    foreach (var column in schema) 
                        valuesStr += reader[column.ColumnName] + "\t";
                    Console.WriteLine(valuesStr);
                }

                //как было
                //Console.WriteLine("ID\trooms\tfloore\tFullSquare\tTenant");
                //while (reader.Read())
                //    Console.WriteLine($"{reader["ID"]}\t{reader["rooms"]}\t" +
                //        $"{reader["floore"]}\t{reader["FullSquare"]}\t\t{reader["Tenant"]}");
            }


        }

        static void Main(string[] args)
        {


            string name;
            using (var connection = new FlatDbConnection())
            {
                do
                {

                    Console.Write("Введите логин:");
                    var login = Console.ReadLine();
                    if (string.IsNullOrEmpty(login))
                    {
                        Console.WriteLine("Astalavista!");
                        return;
                    }

                    name = login.ToString();
                    var sql = $"select * from User where login = '{login}'";
                    using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("Wrong login!");
                            continue;
                        }


                        Console.Write("Введите пароль:");
                        var password = Console.ReadLine();
                        reader.Read();
                        if (reader["password"].ToString() != password)
                        {
                            Console.WriteLine("Wrong password!");
                            continue;
                        }
                    }
                    Console.WriteLine($"Добро пожаловать, {login}!");
                    break;
                } while (true);
            }


            Console.Write("Просмотреть все результаты FullSquare введите Y:");
            char a = char.Parse(Console.ReadLine());
            if ((a == 'y') || (a == 'Y')) 
                showFlets("select * from Flat order by 'id'");
            else
            {
                Console.Write("Тогда введите диапазон FullSquare (min) пробел (max):");
                var str = Console.ReadLine().Split();
                int min = int.Parse(str[0]);
                int max = int.Parse(str[1]);
                showFlets($"select * from Flat where FullSquare < {max} and  FullSquare > {min} order by id");
            }

            Console.Write("Какой вариант подходит? Введите 'id':");
            var idFlat = Console.ReadLine();

            using (var connection = new FlatDbConnection())
            {
                var sql = $"update Flat set Tenant = '{name}' WHERE id = {idFlat}";
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }
            
            showFlets($"select * from Flat where id = {idFlat}");
        }
    }
}