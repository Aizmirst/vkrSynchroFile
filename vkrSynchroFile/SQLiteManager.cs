using System.Collections.ObjectModel;
using System.Data.SQLite;

namespace vkrSynchroFile
{
    internal class SQLiteManager
    {
        private SQLiteConnection connection = new SQLiteConnection("Data Source=SynhroFileDatabase.db;Version=3;");

        public SQLiteManager()
        {
            connection.Open();
            string createFoldersTable = "CREATE TABLE IF NOT EXISTS Folders (" +
                                        "id_folder INTEGER PRIMARY KEY AUTOINCREMENT," +
                                        "folder_name TEXT NOT NULL," +
                                        "folder_path TEXT NOT NULL)";

            string createPCProfilesTable = "CREATE TABLE IF NOT EXISTS PC_Profiles (" +
                                            "id_profile INTEGER PRIMARY KEY AUTOINCREMENT," +
                                            "folder1 INTEGER NOT NULL," +
                                            "folder2 INTEGER NOT NULL," +
                                            "two_sided BOOLEAN NOT NULL," +
                                            "auto_type BOOLEAN NOT NULL," +
                                            "auto_day TEXT," +
                                            "auto_time TEXT," +
                                            "FOREIGN KEY (folder1) REFERENCES Folders(id_folder)," +
                                            "FOREIGN KEY (folder2) REFERENCES Folders(id_folder))";

            string createInternetProfilesTable = "CREATE TABLE IF NOT EXISTS Internet_Profiles (" +
                                            "id_profile INTEGER PRIMARY KEY AUTOINCREMENT," +
                                            "folder INTEGER NOT NULL," +
                                            "id_user TEXT NOT NULL," +
                                            "profile_UID TEXT NOT NULL," +
                                            "two_sided BOOLEAN NOT NULL," +
                                            "mainUser BOOLEAN NOT NULL," +
                                            "auto_type BOOLEAN NOT NULL," +
                                            "auto_day TEXT," +
                                            "auto_time TEXT," +
                                            "FOREIGN KEY (folder) REFERENCES Folders(id_folder))";

            SQLiteCommand command = new SQLiteCommand(createFoldersTable, connection);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(createPCProfilesTable, connection);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(createInternetProfilesTable, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public bool IsMainUser(string profileUID)
        {
            bool isMainUser = false;
            connection.Open();

            string query = "SELECT mainUser FROM Internet_Profiles WHERE profile_UID = @ProfileUID";

            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ProfileUID", profileUID);

            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                isMainUser = reader.GetBoolean(0);
            }

            reader.Close();
            connection.Close();

            return isMainUser;
        }

        public string getFolderPathInternetProfile(string profileUID)
        {
            string folderPath = "";
            connection.Open();

            string query = "SELECT " +
                "f1.folder_path AS folder1_path " +
                "FROM " +
                "Internet_Profiles p " +
                "JOIN " +
                "Folders f1 ON p.folder = f1.id_folder " +
                "WHERE " +
                "p.profile_UID = @ProfileUID";

            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ProfileUID", profileUID);

            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                folderPath = reader["folder1_path"].ToString();
            }

            reader.Close();
            connection.Close();

            return folderPath;
        }

        public ObservableCollection<ListItem> readDBforTable()
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();

            int profileNumber = 1; // Начальное значение номера профиля

            foreach (ListItem item in readDBforTablePC(profileNumber))
            {
                items.Add(item);
                profileNumber++; // Увеличиваем номер профиля на 1
            }
            foreach (ListItem item in readDBforTableInternet(profileNumber))
            {
                items.Add(item);
                profileNumber++; // Увеличиваем номер профиля на 1
            }

            return items;

        }

        private ObservableCollection<ListItem> readDBforTableInternet(int startProfileNumber)
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();

            string info = "SELECT " +
                "p.id_profile, " +
                "f1.id_folder AS folder1_id, " +
                "f1.folder_name AS folder1_name, " +
                "f1.folder_path AS folder1_path, " +
                "p.id_user," +
                "p.profile_UID," +
                "p.two_sided, " +
                "p.mainUser, " +
                "p.auto_type, " +
                "p.auto_day, " +
                "p.auto_time " +
                "FROM " +
                "Internet_Profiles p " +
                "JOIN " +
                "Folders f1 ON p.folder = f1.id_folder ";

            connection.Open();
            SQLiteCommand command = new SQLiteCommand(info, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ListItem()
                {
                    profile_id = reader.GetInt32(0),
                    folder1id = reader.GetInt32(1),
                    folder1name = reader.GetString(2),
                    folder1path = reader.GetString(3),
                    userUID = reader.GetString(4),
                    profileUID = reader.GetString(5),
                    text = $"Профиль №{startProfileNumber}. Тип: По локальной сети. UID: {reader.GetString(5)}",
                    profType = 3,
                    profMode = reader.GetBoolean(6),
                    mainUser = reader.GetBoolean(7),
                    auto_type = reader.GetBoolean(8),
                    auto_day = reader.GetString(9),
                    auto_time = reader.GetString(10)
                });
                startProfileNumber++; // Увеличиваем номер профиля на 1
            }
            connection.Close();
            return items;
        }

        private ObservableCollection<ListItem> readDBforTablePC(int startProfileNumber)
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();

            string info = "SELECT " +
                "p.id_profile, " +
                "f1.id_folder AS folder1_id, " +
                "f1.folder_name AS folder1_name, " +
                "f1.folder_path AS folder1_path, " +
                "f2.id_folder AS folder2_id, " +
                "f2.folder_name AS folder2_name, " +
                "f2.folder_path AS folder2_path, " +
                "p.two_sided, " +
                "p.auto_type, " +
                "p.auto_day, " +
                "p.auto_time " +
                "FROM " +
                "PC_Profiles p " +
                "JOIN " +
                "Folders f1 ON p.folder1 = f1.id_folder " +
                "JOIN " +
                "Folders f2 ON p.folder2 = f2.id_folder;";

            connection.Open();
            SQLiteCommand command = new SQLiteCommand(info, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new ListItem()
                {
                    profile_id = reader.GetInt32(0),
                    folder1id = reader.GetInt32(1),
                    folder1name = reader.GetString(2),
                    folder1path = reader.GetString(3),
                    folder2id = reader.GetInt32(4),
                    folder2name = reader.GetString(5),
                    folder2path = reader.GetString(6),
                    text = $"Профиль №{startProfileNumber}. Тип: Внутри 1 ПК.",
                    profType = 1,
                    profMode = reader.GetBoolean(7),
                    auto_type = reader.GetBoolean(8),
                    auto_day = reader.GetString(9),
                    auto_time = reader.GetString(10)
                });
                startProfileNumber++; // Увеличиваем номер профиля на 1
            }
            connection.Close();
            return items;
        }

        public void insertDB(bool two_sided, string name1, string path1, string name2, string path2, bool auto_type, string auto_day, string auto_time)
        {
            int lastInsertedId1 = insertFolder(name1, path1);
            int lastInsertedId2 = insertFolder(name2, path2);
            insertProfile(lastInsertedId1, lastInsertedId2, two_sided, auto_type, auto_day, auto_time);
        }

        public void insertInternetDB(bool two_sided, string name, string path, string id_user, string profile_UID, bool mainUser, bool auto_type, string auto_day, string auto_time)
        {
            int lastInsertedId = insertFolder(name, path);
            insertInternetProfile(lastInsertedId, id_user, profile_UID, two_sided, mainUser, auto_type, auto_day, auto_time);
        }

        private int insertFolder(string name, string path)
        {
            string sql = "INSERT INTO Folders (folder_name, folder_path) " +
             "VALUES (@folder_name, @folder_path); SELECT last_insert_rowid();";

            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder_name", name);
            cmd.Parameters.AddWithValue("@folder_path", path);
            int lastInsertedId = Convert.ToInt32(cmd.ExecuteScalar());
            connection.Close();

            return lastInsertedId;
        }

        private void insertProfile(int id1, int id2, bool two_sided, bool auto_type, string auto_day, string auto_time)
        {
            connection.Open();
            string sql = "INSERT INTO PC_Profiles (folder1, folder2, two_sided, auto_type, auto_day, auto_time) VALUES (@folder1, @folder2, @two_sided, @auto_type, @auto_day, @auto_time);";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder1", id1);
            cmd.Parameters.AddWithValue("@folder2", id2);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.Parameters.AddWithValue("@auto_type", auto_type);
            cmd.Parameters.AddWithValue("@auto_day", auto_day);
            cmd.Parameters.AddWithValue("@auto_time", auto_time);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void insertInternetProfile(int id, string id_user, string profile_UID, bool two_sided, bool mainUser, bool auto_type, string auto_day, string auto_time)
        {
            connection.Open();
            string sql = "INSERT INTO Internet_Profiles (folder, id_user, profile_UID, two_sided, mainUser, auto_type, auto_day, auto_time) VALUES (@folder, @id_user, @profile_UID, @two_sided, @mainUser, @auto_type, @auto_day, @auto_time);";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder", id);
            cmd.Parameters.AddWithValue("@id_user", id_user);
            cmd.Parameters.AddWithValue("@profile_UID", profile_UID);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.Parameters.AddWithValue("@mainUser", mainUser);
            cmd.Parameters.AddWithValue("@auto_type", auto_type);
            cmd.Parameters.AddWithValue("@auto_day", auto_day);
            cmd.Parameters.AddWithValue("@auto_time", auto_time);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void deleteDB_PC(int profId, int fold1Id, int fold2Id)
        {
            deleteProfilePC(profId);
            deleteFolder(fold1Id);
            deleteFolder(fold2Id);
        }

        public void deleteDB_Internet(string userUID, string profUID)
        {
            int folderID = deleteProfileInternet(userUID, profUID);
            if (folderID > 0)
            {
                deleteFolder(folderID);
            }
        }

        private int deleteProfileInternet(string userUID, string profUID)
        {
            int deletedFolderId = -1; // Инициализируем значение переменной folder удаляемой строки

            try
            {
                // SQL-запрос для выбора значения folder перед удалением записи
                string selectFolderSql = $"SELECT folder FROM Internet_Profiles WHERE id_user = '{userUID}' AND profile_UID = '{profUID}'";

                // SQL-запрос для удаления записи
                string deleteSql = $"DELETE FROM Internet_Profiles WHERE id_user = '{userUID}' AND profile_UID = '{profUID}'";

                // Открытие соединения с базой данных
                connection.Open();

                // Получение значения folder перед удалением записи
                SQLiteCommand selectFolderCmd = new SQLiteCommand(selectFolderSql, connection);
                object folderObj = selectFolderCmd.ExecuteScalar();
                if (folderObj != null && folderObj != DBNull.Value)
                {
                    deletedFolderId = Convert.ToInt32(folderObj);
                }

                // Выполнение SQL-запроса на удаление записи
                SQLiteCommand deleteCmd = new SQLiteCommand(deleteSql, connection);
                deleteCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении записи из базы данных: " + ex.Message);
            }
            finally
            {
                // Закрытие соединения с базой данных
                connection.Close();
            }

            // Возвращаем значение folder удаленной строки
            return deletedFolderId;
        }

        private void deleteProfilePC(int profId)
        {
            string sql = $"DELETE FROM PC_Profiles WHERE id_profile='{profId}';";
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void deleteFolder(int folderId)
        {
            string sql = $"DELETE FROM Folders WHERE id_folder='{folderId}';";
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void updateDB_Internet(int folderId, string name, string path)
        {
            updateFolder(folderId, name, path);
        }

        public void updateDB_PC(int profile_id, bool two_sided,
            int folder1Id, string name1, string path1,
            int folder2Id, string name2, string path2, bool auto_type, string auto_day, string auto_time)
        {
            updateFolder(folder1Id, name1, path1);
            updateFolder(folder2Id, name2, path2);
            updateProfile(profile_id, two_sided, auto_type, auto_day, auto_time);
        }

        private void updateFolder(int id, string name, string path)
        {
            string sql = "UPDATE Folders SET " +
                "folder_name = @folder_name, " +
                "folder_path = @folder_path " +
                "WHERE " +
                "id_folder = @id_folder;";
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder_name", name);
            cmd.Parameters.AddWithValue("@folder_path", path);
            cmd.Parameters.AddWithValue("@id_folder", id);
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void updateProfile(int profile_id, bool two_sided, bool auto_type, string auto_day, string auto_time)
        {
            string sql = "UPDATE PC_Profiles SET " +
                "two_sided = @two_sided, " +
                "auto_type = @auto_type, " +
                "auto_day = @auto_day, " +
                "auto_time = @auto_time, " +
                "WHERE " +
                "id_profile = @id_profile;";
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.Parameters.AddWithValue("@id_profile", profile_id);
            cmd.Parameters.AddWithValue("@auto_type", auto_type);
            cmd.Parameters.AddWithValue("@auto_day", auto_day);
            cmd.Parameters.AddWithValue("@auto_time", auto_time);
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}
