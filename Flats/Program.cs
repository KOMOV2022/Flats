using System.Data.SQLite;

namespace Flats
{
    class Program
    {
        private static string? priznak;
        private static string? name;

        static void connectDbAddSql(string sql)
        {
            using (var connection = new FlatDbConnection())
            {
                using (var command = new SQLiteCommand(sql, connection.Sqlite))
                    command.ExecuteNonQuery();
            }
        }

        static void connectDbAdd(int ROOMS,int FLOORE,int FULLSQUARE) 
        {
            connectDbAddSql($"INSERT INTO Flat (ROOMS, FLOORE,   FULLSQUARE,   TENANT) " +
                $"VALUES ({ROOMS}, {FLOORE} {FULLSQUARE}, null)");
        }

        static void connectDbDel(int id) 
        {
            connectDbAddSql($"DELETE FROM Flat WHERE id = '{id}'");
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
                    while (reader.Read())
                        someData = reader.GetString(0);

            }
            return someData;
        }

        static void showFletss() 
        {
            showFletsBySql($"SELECT * FROM Flat ");
        }

        static void showFletsBySql(string sql)
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

        static void showFletsBySqare(int min,int max)
        {
            showFletsBySql($"select *" + $" from Flat where (Tenant = '{name}' OR Tenant is NULL) and FullSquare < {max}" +
                                        $" and FullSquare > {min} order by id ");

        }

        static void showFletsByUser()
        {
            showFletsBySql($"SELECT * FROM Flat " +
                            $"WHERE Tenant = '{name}' OR Tenant is NULL");
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

        static void FlatsViewModeByFullSquare()
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
                        showFletsBySqare(min, max);
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

        static void FlatsViewMode()
        {
            do
            {
                Console.Write("Просмотреть все доступные для аренды варианты квартир введите Y/N:");
                bool success = char.TryParse(Console.ReadLine().ToLower(), out char symbol);
                if (success && (symbol == 'y' || symbol == 'n'))
                {
                    if (symbol == 'y')
                        showFletsByUser();
                    else if (symbol == 'n')
                        FlatsViewModeByFullSquare();
                 break;
                }
                else
                    Console.WriteLine("Неверный символ!");
            } while (true);
        }

        private static void Book(int idFlat)
        {
            connectDbAddSql($"update Flat set Tenant = '{name}' WHERE id = {idFlat}");
        }

        private static void AdminStuffAddValid()
        {
            var adStr = Console.ReadLine().Split();

            if (adStr.Length == 3)
            {
                var adInt = new int[adStr.Length + 1];
                for (int i = 1; i <= adStr.Length; i++)
                {
                    bool success = int.TryParse(adStr[i - 1], out int num);
                    adInt[i] = num;
                }
                if ((adInt[1] < 4 && adInt[1] > 0)
                    && (adInt[2] > 0 && adInt[2] < 19)
                    && (adInt[3] > 15 && adInt[3] < 100))
                {
                    connectDbAdd(adInt[1], adInt[2], adInt[3]);
                    showFletss();
                }
                else
                {
                    Console.WriteLine($"Значения некорректны." +
                        $" Будте внимательнее!");
                }
            }
        }

        private static void AdminStuff()
        {
            Console.WriteLine($"Чем займёмся {name}");
            showFletss();

            while (true)
            {
                Console.Write("Чтобы добавить запись введите 'add' чтобы удалить 'del': ");
                string? str = Console.ReadLine();
                if (str == "add")
                {
                    Console.Write($"Введите значения полей через пробел" +
                        $" (rooms (от 1 до 3) floore (от 1 до 18) FullSquare(от 16 до 99)): ");
                    AdminStuffAddValid();
                }
                else if (str == "del")
                {
                    Console.Write("Id строки которую хотите удалить: ");
                    int del = int.Parse(Console.ReadLine());
                    connectDbDel(del);
                    showFletss();
                }
                else return;
            }

        }

        static void Main(string[] args) 
        {
            while (true)
            {
                autorise();
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
                    var idFlatStr = Console.ReadLine();
                    if(int.TryParse(idFlatStr, out var idFlat)) Book(idFlat);
                    else
                    {
                        Console.WriteLine($"Некорректный ввод. '{idFlatStr}' не является идентификатором.");
                        break;
                    }
                    showFletsByUser();
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