﻿using System.Data.SQLite;

namespace Flats
{
    class Program
    {
        //todo избавляемся от sql внутри Main. Выносим его в методы, для каждого отличного от других SQL-запроса отдельный метод.
        //todo зарефачить так чтобы методы были не длиннее 30 строк, классы не длиннее 300.
        //todo после бронирования давать выбор - бронировать ещё или выйти

        static void connectDb(string sql) //review параметр убрать, SQL захардкодить внутри метода
        {
            using (var connection = new FlatDbConnection())
            {

                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }

        }
        static string? priznakAdmin(string sql) //review параметр убрать, SQL захардкодить внутри метода
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
        static int connectDbReturn(string sql) //review параметр убрать, SQL захардкодить внутри метода
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

        static void showFlets(string sql) //review параметр убрать, SQL захардкодить внутри метода
        {
            using (var connection = new FlatDbConnection())
            using (var command = new SQLiteCommand(sql, connection.Sqlite))
            using (var reader = command.ExecuteReader())
            {
                //review если запрос ничего не вернул, мы тут всё равно показываем заголовки. Для пользователя непонятно.
                //Надо писать "Нет даных", или что-то такое
                Console.WriteLine($"ID  ROOMS   FLOORE   FULLSQUARE   TENANT");
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["ID"]}\t{reader["rooms"]}\t" +
                       $"{reader["floore"]}\t{reader["FullSquare"]}\t  {reader["Tenant"]}");

                }
            }

        }

        //review вот тут хорошо - SQL спрятан внутри метода.
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
        //review длинный метод
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
                //review ввод пароля - хороший кандидат на вынос в отдельный метод.
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
                    else if (key.Key != ConsoleKey.Backspace)   //review не вижу результат стирания символов
                                                                //По этой теме:
                                                                //https://stackoverflow.com/questions/3404421/password-masking-console-application
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

        //review длинный метод
        //review То, что делает метод, должно быть легко объяснить в паре слов. Здесь мне непонятно.
        static void choice()    
        {
            //review Текст непонятный. Показывать тут текст, который бы понял пользователь, видящий наш интерфейс впервые
            Console.Write("Просмотреть все результаты FullSquare введите Y:"); 
            char a = char.Parse(Console.ReadLine());    //review Тут крашится. Используй TryParse для валидации ввода. 
                                                        //или можно сделать ReadKey вместо ReadLine
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
                    var str = Console.ReadLine().Split();   //review проверяй, что длина массива больше или равна 2
                    int min = int.Parse(str[0]);    //review Тут крашится. Используй TryParse для валидации ввода. 
                    int max = int.Parse(str[1]);    //review Тут крашится. Используй TryParse для валидации ввода. 
                    showFlets($"select *" +
                        $" from Flat where FullSquare < {max}" +
                        $" and FullSquare > {min} order by id"); //review тут всё ещё можно увидеть квартиры, забронированные другими

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
        static void Main(string[] args) //review длинный метод
        {
            while (true)
            {
                string name = autorise();
                priznak = priznakAdmin($"SELECT big_boss FROM User WHERE login = '{name}'");
                if (priznak == "master")
                    name = "admin";     //review менять name плохо, потому что сбивает с толку -
                                        //ведь имя у админа может быть и отличное от admin.
                                        //Кроме того, это не нужно, потому что можно на строке
                                        //if (name == "admin")
                                        //проверять признак, а не name

                else if (name == null)
                    return;

                if (name == "admin")    //review админские движухи внутри этого условия - хороший кандидат на вынос в отдельный метод
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
                                adInt[i] = int.Parse(adStr[i - 1]); //review нужна валидация ввода. 
                                                                    //Сейчас при любом некорректном вводе будет необработанное исключение
                                                                    //используй TryParse

                            if ((adInt[1] < 4 && adInt[1] > 0)
                                && (adInt[2] > 0 && adInt[2] < 19)
                                && (adInt[3] > 15 && adInt[3] < 100))
                            {
                                string tenant = "null"; //review зачем нужна эта переменная?
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
                    continue;       //review перенеси continue в конец if (name == "admin"), который чуть выше
                                    //потому что это по логике это одно и то же условие, так зачем два ифа?

                choice();


                while (true)
                {

                    Console.Write("Какой вариант подходит? Введите 'id':");
                    var idFlat = Console.ReadLine();
                    //review вынести в метод
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