using System.Data.SQLite;

namespace Flats
{
    class Program
    {
        //todo режим риэлтора. У него есть полномочия добавлять/удалять квартиры
        //и он видит все квартиры
        //todo скопировать базу на DNS машинуp;o
        static void showFlets(string sql)
        {
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("ID    rooms   floore  FullSquare  Tenant");
                while (reader.Read())
                    Console.WriteLine($"{reader["ID"]}\t{reader["rooms"]}\t" +
                       $"{reader["floore"]}\t{reader["FullSquare"]}\t  {reader["Tenant"]}");
            }
        }

        static string autorise()
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
                        return null;
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
            return name;


        }

        static void choice()
        {
            Console.Write("Просмотреть все результаты FullSquare введите Y:");

            char a = char.Parse(Console.ReadLine());
            if ((a == 'y') || (a == 'Y'))
                showFlets("select ID, rooms, floore, FullSquare, Tenant from Flat order by 'id'");
            else
            {
                Console.Write("Тогда введите диапазон FullSquare (min) пробел (max):");
                var str = Console.ReadLine().Split();
                int min = int.Parse(str[0]);
                int max = int.Parse(str[1]);
                showFlets($"select * from Flat where FullSquare between {max} and {min} order by id");
            }

        }

        static void Main(string[] args)
        {           
           string name = autorise();
            if (name == null)
                return;
            choice();

            Console.Write("Какой вариант подходит? Введите 'id':");
            var idFlat = Console.ReadLine();

            using (var connection = new FlatDbConnection())
            {
                var sql = $"update Flat set Tenant = '{name}' WHERE id = {idFlat}";
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }

            showFlets($"select * from Flat where Tenant = '{name}' or Tenant is null");
        }
    }
}