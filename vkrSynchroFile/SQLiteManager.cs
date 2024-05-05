using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Windows.Documents;

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
                                            "FOREIGN KEY (folder1) REFERENCES Folders(id_folder)," +
                                            "FOREIGN KEY (folder2) REFERENCES Folders(id_folder))";

            string createInternetProfilesTable = "CREATE TABLE IF NOT EXISTS Internet_Profiles (" +
                                            "id_profile INTEGER PRIMARY KEY AUTOINCREMENT," +
                                            "folder INTEGER NOT NULL," +
                                            "id_user TEXT NOT NULL," +
                                            "profile_UID TEXT NOT NULL," +
                                            "two_sided BOOLEAN NOT NULL," +
                                            "FOREIGN KEY (folder) REFERENCES Folders(id_folder))";

            SQLiteCommand command = new SQLiteCommand(createFoldersTable, connection);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(createPCProfilesTable, connection);
            command.ExecuteNonQuery();

            command = new SQLiteCommand(createInternetProfilesTable, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }


        public ObservableCollection<ListItem> readDBforTable()
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();

            foreach (ListItem item in readDBforTablePC())
            {
                items.Add(item);
            }
            foreach (ListItem item in readDBforTableInternet())
            {
                items.Add(item);
            }

            return items;

        }

        private ObservableCollection<ListItem> readDBforTableInternet()
        {
            ObservableCollection<ListItem> items = new ObservableCollection<ListItem>();

            string info = "SELECT " +
                "p.id_profile, " +
                "f1.id_folder AS folder1_id, " +
                "f1.folder_name AS folder1_name, " +
                "f1.folder_path AS folder1_path, " +
                "p.id_user," +
                "p.profile_UID," +
                "p.two_sided " +
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
                    text = $"Профиль №{reader.GetInt32(0)}. Тип: По сети.",
                    profType = 3,
                    profMode = reader.GetBoolean(6)
                });
            }
            connection.Close();
            return items;
        }
        
        private ObservableCollection<ListItem> readDBforTablePC()
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
                "p.two_sided " +
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
                    text = $"Профиль №{reader.GetInt32(0)}. Тип: Внутри 1 ПК.",
                    profType = 1,
                    profMode = reader.GetBoolean(7)
                });
            }
            connection.Close();
            return items;
        }

        public void insertDB(bool two_sided, string name1, string path1, DateTime changeTime1, long weight1, string name2, string path2, DateTime changeTime2, long weight2)
        {
            int lastInsertedId1 = insertFolder(name1, path1, changeTime1, weight1);
            int lastInsertedId2 = insertFolder(name2, path2, changeTime2, weight2);
            insertProfile(lastInsertedId1, lastInsertedId2, two_sided);
        }

        public void insertInternetDB(bool two_sided, string name, string path, DateTime changeTime, long weight, string id_user, string profile_UID)
        {
            int lastInsertedId = insertFolder(name, path, changeTime, weight);
            insertInternetProfile(lastInsertedId, id_user, profile_UID, two_sided);
        }

        private int insertFolder(string name, string path, DateTime changeTime, long weight)
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

        private void insertProfile(int id1, int id2, bool two_sided)
        {
            connection.Open();
            string sql = "INSERT INTO PC_Profiles (folder1, folder2, two_sided) VALUES (@folder1, @folder2, @two_sided);";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder1", id1);
            cmd.Parameters.AddWithValue("@folder2", id2);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void insertInternetProfile(int id, string id_user, string profile_UID, bool two_sided)
        {
            connection.Open();
            string sql = "INSERT INTO Internet_Profiles (folder, id_user, profile_UID, two_sided) VALUES (@folder, @id_user, @profile_UID, @two_sided);";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@folder", id);
            cmd.Parameters.AddWithValue("@id_user", id_user);
            cmd.Parameters.AddWithValue("@profile_UID", profile_UID);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void deleteDB(int profId, int fold1Id, int fold2Id)
        {
            deleteProfile(profId);
            deleteFolder(fold1Id);
            deleteFolder(fold2Id);
        }

        private void deleteProfile(int profId)
        {
            string sql = $"DELETE FROM PC_Profiles WHERE id_profile='{profId}';";
            //Console.WriteLine(sql);
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        private void deleteFolder(int folderId)
        {
            string sql = $"DELETE FROM Folders WHERE id_folder='{folderId}';";
            //Console.WriteLine(sql);
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }



        public void updateDB(int profile_id, bool two_sided,
            int folder1Id, string name1, string path1,
            int folder2Id, string name2, string path2) //более правильно и безопаснее, но чуть геморнее, ибо надо учитывать типы
        {
            updateFolder(folder1Id, name1, path1);
            updateFolder(folder2Id, name2, path2);
            updateProfile(profile_id, two_sided);
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

        private void updateProfile(int profile_id, bool two_sided)
        {
            string sql = "UPDATE PC_Profiles SET " +
                "two_sided = @two_sided " +
                "WHERE " +
                "id_profile = @id_profile;";
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@two_sided", two_sided);
            cmd.Parameters.AddWithValue("@id_profile", profile_id);
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}
