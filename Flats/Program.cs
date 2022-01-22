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
                showFlets($"select * from Flat where FullSquare < {max} and FullSquare > {min} order by id");
            }

        }

        static void Main(string[] args)
        {
            while (true)
            {
                string name = autorise();
                if (name == null)
                    return;

                if (name == "admin")
                {
                    Console.WriteLine($"Чем займёмся " +
                        $"" +
                        $"{name}");
                    showFlets("select * from Flat order by 'id'");

                    while (true)
                    {
                        Console.Write("Чтобы добавить запись введите 'add' чтобы удалить 'del': ");
                        string? str = Console.ReadLine();
                        if (str == "add")
                        {
                            Console.Write($"Введите значения полей через пробел" +
                                $" (id rooms floore FullSquare Tenant): ");
                            var adStr = Console.ReadLine().Split();
                            int[] adInt = new int[adStr.Length - 1];
                            for (int i = 0; i < adStr.Length - 1; i++)
                                adInt[i] = int.Parse(adStr[i]);

                            showFlets($"INSERT INTO Flat VALUES" +
                                $" ({adInt[0]}, {adInt[1]}, {adInt[2]}, {adInt[3]}, {adStr[4]});");
                            showFlets("select * from Flat order by 'id'");
                            continue;
                        }
                        else if (str == "del")
                        {
                            Console.Write("Id строки которую хотите удалить: ");
                            int del = int.Parse(Console.ReadLine());
                            showFlets($"DELETE FROM Flat WHERE id = '{del}'");
                            showFlets("select * from Flat order by 'id'");
                            continue;
                        }
                        else break;
                    }

                }
                if (name == "admin")
                    continue;

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
                return;
            }
        }
    }
}