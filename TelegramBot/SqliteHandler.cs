using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;


namespace TelegramBot
{
    class SqliteHandler
    {
        static string dbName = "Vk2Tg.db";
        static string dbPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + dbName; // Бд лежит в папке с исполняемым файлом

        // Создаем и инициализируем бд при необходимости.
        static SqliteHandler()
        {
            if (!File.Exists(dbPath)) // Если бд нету.
            {
                SQLiteConnection.CreateFile(dbPath); // Создать базу данных, по указанному пути содаётся пустой файл бд.
            }

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    // Строка запроса, который надо будет выполнить
                    string commandText =
                        "CREATE TABLE IF NOT EXISTS relation(" +
                        "vkGroupName TEXT," +
                        "tgChannelId INTEGER," +
                        "lastPostId INTEGER DEFAULT 0);"; // Создать таблицу, если её нет.

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery(); // Выполнить запрос
                }

                connection.Close();
            }
        }

        // Добавляет в бд имя группы vk и id tg канала.
        public static void AddVkToTgRelation(string vkGroupName, int tgChannelId)
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    string commandText =
                        $"INSERT INTO relation(vkGroupName, tgChannelId) VALUES('{vkGroupName}', {tgChannelId});";

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public static void RemoveVkToTgRelation(string vkGroupName, int tgChannelId)
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    // Строка запроса, который надо будет выполнить
                    string commandText =
                    "DELETE FROM relation " +
                    $"WHERE vkGroupName = '{vkGroupName}' AND tgChannelId = {tgChannelId};";

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery(); // Выполнить запрос
                }

                connection.Close();
            }
        }

        // Возвращает полную таблицу relation из бд.
        public static List<SqliteRelationTable> GetFullSqliteTable()
        {
            List<SqliteRelationTable> relationTable = new List<SqliteRelationTable>();

            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    string commandText =
                    "SELECT * FROM relation ORDER BY vkGroupName ASC;";

                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;

                    var output = command.ExecuteReader();
                    while (output.Read())
                    {
                        relationTable.Add(new SqliteRelationTable(output.GetString(0), output.GetInt32(1), output.GetInt32(2)));
                    }

                    output.Close();
                }

                connection.Close();

                return relationTable;
            }
        }

        // Обновляет id последнего поста группы vk.
        public static void UpdateLastId(string vkGroupName, int id)
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={dbPath}; Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    string commandText =
                    $"UPDATE relation SET lastPostId = {id} " +
                    $"WHERE vkGroupName = '{vkGroupName}'";
                    command.CommandText = commandText;
                    command.CommandType = CommandType.Text;

                    var output = command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}
