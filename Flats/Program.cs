using System.Data.SQLite;
//using System.SQLite.;

namespace Flats
{
    class Program
    {
        //todo валидация на функцию add (добавление квартир) *

        //todo не-админ вообще не должен иметь возможности *
        //увидеть забронированные другими квартиры *

        //todo после бронирования давать выбор - бронировать ещё или выйти
        //todo пароль не должен быть виден, когда его набираешь *

        //todo Ситуация. Я отфильтровал квартиры по площади, и не вижу там той, *
        //которую хочу забронировать.Я теперь хочу вывести все квартиры,чтобы найти её. *
        //Я не могу это сделать, пока не поставлю бронь. А такая возможность нужна. *


        //todo сделать айдишник автоинкрементным *

        //todo добавить признак администратора в таблицу User, чтобы любого
        //юзера можно было сделать админом или наоборот

        //todo избавляемся от sql внутри Main. Выносим его в методы: свой для select,
        //свой для delete и т.д. (а сейчас для всего используется showFlets, и это не очень)

        //todo зарефачить так чтобы методы были не длиннее 30 строк, классы не длиннее 300.
        static void connectDb(string sql)
        {
            using (var connection = new FlatDbConnection())
            {

                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }

        }
        static string? priznakAdmin(string sql)
        {
            string? someData = "slave";
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                    while(reader.Read())
                        someData = reader.GetString(0);

            }
            return someData;
        }
        static int connectDbReturn(string sql)
        {
            int someData = 0;
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    someData = reader.GetInt32(0);
            }
            return someData;
        }

        static void showFlets(string sql)
        {
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine($"ID  ROOMS   FLOORE   FULLSQUARE   TENANT");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["ID"]}\t{reader["rooms"]}\t" +
                       $"{reader["floore"]}\t{reader["FullSquare"]}\t  {reader["Tenant"]}");

                }
            }

        }

        static string? GetUserPassword(string login)
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
        static string? name;
        static string autorise()
        {
            do
            {
                Console.Write("Введите логин:");
                name = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("Astalavista!");
                    return null;
                }
                var passwordFromDb = GetUserPassword(name);
                if (passwordFromDb == null)
                {
                    Console.WriteLine("Wrong login!");
                    continue;
                }
                Console.Write("Введите пароль:");
                var password = "";
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.Write("\n");
                        break;
                    }
                    else if (key.Key != ConsoleKey.Backspace)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                }
                while (true);
                if (passwordFromDb != password)
                {
                    Console.WriteLine("Wrong password!");
                    continue;
                }
                Console.WriteLine($"Добро пожаловать, {name}!");
                break;
            } while (true);
            return name;
        }

        static void choice()
        {
           
                Console.Write("Просмотреть все результаты FullSquare введите Y:");
                char a = char.Parse(Console.ReadLine());
            while (true)
            {
                if ((a == 'y') || (a == 'Y'))
                {
                    showFlets($"SELECT * FROM Flat " +
                        $"WHERE Tenant = '{name}' OR Tenant is NULL");
                    break;

                }
                else
                {
                    Console.Write("Тогда введите диапазон FullSquare (min) пробел (max):");
                    var str = Console.ReadLine().Split();
                    int min = int.Parse(str[0]);
                    int max = int.Parse(str[1]);
                    showFlets($"select *" +
                        $" from Flat where FullSquare < {max}" +
                        $" and FullSquare > {min} order by id");

                    Console.Write("Если нет вариантов вернёмся к " +
                            "полному списку y/n:");
                    string y_n = Console.ReadLine();
                    if (y_n == "y")
                        showFlets($"SELECT * FROM Flat " +
                            $"WHERE Tenant = '{name}' OR Tenant is NULL");
                    else
                        break;

                }
            }
        }


        static string? priznak;
        static void Main(string[] args)
        {
            while (true)
            {
                string name = autorise();
                priznak = priznakAdmin($"SELECT big_boss FROM User WHERE login = '{name}'");
                if (priznak == "master")
                    name = "admin";

                else if (name == null)
                    return;

                if (name == "admin")
                {
                    Console.WriteLine($"Чем займёмся {name}");
                    showFlets("select * from Flat order by 'id'");

                    while (true)
                    {
                        Console.Write("Чтобы добавить запись введите 'add' чтобы удалить 'del': ");
                        string? str = Console.ReadLine();
                        if (str == "add")
                        {
                            Console.Write($"Введите значения полей через пробел" +
                                $" (rooms (от 1 до 3) floore (от 1 до 18) FullSquare(от 16 до 99)): ");
                            var adStr = Console.ReadLine().Split();
                            int?[] adInt = new int?[adStr.Length + 1];
                            for (int i = 1; i <= adStr.Length; i++)
                                adInt[i] = int.Parse(adStr[i - 1]);

                            if ((adInt[1] < 4 && adInt[1] > 0)
                                && (adInt[2] > 0 && adInt[2] < 19)
                                && (adInt[3] > 15 && adInt[3] < 100))
                            {
                                string tenant = "null";
                                int lostRowsId = connectDbReturn($"SELECT id from Flat" +
                                    $" order by id desc limit 1 ");
                                connectDb($"INSERT INTO Flat VALUES" +
                                   $" ({lostRowsId + 1},{adInt[1]}, {adInt[2]}," +
                                   $" {adInt[3]}, {tenant})");
                                showFlets("select * from Flat order by 'id'");
                                continue;
                            }
                            else
                            {
                                Console.WriteLine($"Значения некорректны." +
                                    $" Будте внимательнее!");
                                continue;
                            }


                        }
                        else if (str == "del")
                        {
                            Console.Write("Id строки которую хотите удалить: ");
                            int del = int.Parse(Console.ReadLine());
                            connectDb($"DELETE FROM Flat WHERE id = '{del}'");
                            showFlets("select * from Flat order by 'id'");
                            continue;
                        }
                        else return;
                    }

                }
                if (name == "admin")
                    continue;

                choice();
                

                while (true)
                {
                    
                    Console.Write("Какой вариант подходит? Введите 'id':");
                    var idFlat = Console.ReadLine();
                    using (var connection = new FlatDbConnection())
                    {
                        var sql = $"update Flat set Tenant = '{name}' WHERE id = {idFlat}";
                        using (var command = new SQLiteCommand(sql, connection.Sqlite))
                            command.ExecuteNonQuery();
                    }
                    showFlets($"select * from Flat where Tenant = '{name}' or Tenant is null");
                    Console.Write("Желаете продолжить? y/n: ");
                    var strFlat = Console.ReadLine();
                    if (strFlat == "y")
                        break;
                    else
                        return;
                }
                
            }
        }
    }
}