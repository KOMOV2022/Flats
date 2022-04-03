using System.Data.SQLite;

namespace Flats
{
    class Program
    {
        //todo избавляемся от sql внутри Main. Выносим его в методы, для каждого отличного от других SQL-запроса отдельный метод.
        //todo зарефачить так чтобы методы были не длиннее 30 строк, классы не длиннее 300.
        //todo после бронирования давать выбор - бронировать ещё или выйти

        static void connectDb(string sql)   //review два метода сделать - один для добавления, другой для удаления,
                                            //И в обоих SQL внутри захардкодить
        {
            using (var connection = new FlatDbConnection())
            {

                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }

        }
        static string? priznakAdmin() 
        {
            string sql = $"SELECT big_boss FROM User WHERE login = '{name}'";
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

        static void showFlets(string sql) //review сделать 3(?) разных метода, везде захардкодить SQL
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
                var password = GetPasswordFromConsole();
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

        private static string GetPasswordFromConsole()
        {
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
                else
                {
                    Console.Write("\b \b");
                    password = password.Remove(password.Length - 1);
                }
            }
            while (true);
            return password;
        }

        static void FlatsViewMode()
        {
            char? a = null;
            do
            {

                Console.Write("Просмотреть все доступные для аренды варианты квартир введите Y/N:");
                bool success = char.TryParse(Console.ReadLine().ToLower(), out char symbol);
                if (success && (symbol == 'y' || symbol == 'n'))
                {
                    
                    a = symbol;
                    if (a == 'y')
                    {
                        showFlets($"SELECT * FROM Flat " +
                            $"WHERE Tenant = '{name}' OR Tenant is NULL");
                    }
                    else if (a == 'n')
                    {
                        bool syclo = false;

                        do
                        {
                            syclo = false;
                            Console.Write("Тогда введите диапазон FullSquare (min) пробел (max):");
                            var str = Console.ReadLine().Split();
                            int min = 0;
                            int max = 0;

                            if (str.Length == 2)
                            {
                                bool succesMin = int.TryParse(str[0], out int numberMin);
                                bool succesMax = int.TryParse(str[1], out int numberMax);
                                if (succesMin && succesMax && (numberMin > 0 && numberMin < 100) && (numberMax > 0 && numberMax < 100))
                                {
                                    min = numberMin;
                                    max = numberMax;

                                    showFlets($"select *" +
                                        $" from Flat where FullSquare < {max}" +
                                        $" and FullSquare > {min} order by id");
                                }
                                else
                                {
                                    Console.WriteLine("Уберите мусор с поля!");
                                    syclo = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неверный ввод!");
                                syclo = true;

                                
                            }

                        } while (syclo);
                    }
                    break;
                }

                else
                {
                    Console.WriteLine("Неверный символ!");
                    continue;
                }

                      //review тут всё ещё можно увидеть квартиры, забронированные другими

            } while (true);
        }   

        private static void Book(string idFlatStr)
        {
            using (var connection = new FlatDbConnection())
            {
                var sql = $"update Flat set Tenant = '{name}' WHERE id = {idFlatStr}";
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }
        }

        private static void AdminStuff()
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
                    int?[] adInt = new int?[adStr.Length + 1]; //review проверять длину массива
                    for (int i = 1; i <= adStr.Length; i++)
                        adInt[i] = int.Parse(adStr[i - 1]); //review нужна валидация ввода. 
                                                            //Сейчас при любом некорректном вводе будет необработанное исключение
                                                            //используй TryParse

                    if ((adInt[1] < 4 && adInt[1] > 0)
                        && (adInt[2] > 0 && adInt[2] < 19)
                        && (adInt[3] > 15 && adInt[3] < 100))
                    {


                        connectDb($"INSERT INTO Flat (ROOMS, FLOORE,   FULLSQUARE,   TENANT) VALUES" +
                           $" ({adInt[1]}, {adInt[2]}," +
                           $" {adInt[3]}, null)");
                        showFlets("select * from Flat order by 'id'");
                        continue;       //review continue ничего не делает, убрать нафиг
                    }
                    else
                    {
                        Console.WriteLine($"Значения некорректны." +
                            $" Будте внимательнее!");
                        continue;   //review continue ничего не делает, убрать нафиг
                    }


                }
                else if (str == "del")
                {
                    Console.Write("Id строки которую хотите удалить: ");
                    int del = int.Parse(Console.ReadLine());
                    connectDb($"DELETE FROM Flat WHERE id = '{del}'");
                    showFlets("select * from Flat order by 'id'");
                    continue;   //review continue ничего не делает, убрать нафиг
                }
                else return;
            }

        }


        static string? priznak;
        static void Main(string[] args) 
        {
            while (true)
            {
                autorise(); //review локальная переменная не нужна - есть статическое поле, и оно уже инициализируется внутри ф-ии autorise
                priznak = priznakAdmin();
                if (priznak == "master")
                {
                    AdminStuff();
                    continue;
                }
                else if (name == null) return;
                FlatsViewMode();
                while (true)
                {
                    Console.Write("Какой вариант подходит? Введите 'id':");
                    var idFlat = Console.ReadLine();
                    Book(idFlat);
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